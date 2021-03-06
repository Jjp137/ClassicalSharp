#include "ServerConnection.h"
#include "BlockPhysics.h"
#include "Game.h"
#include "Drawer2D.h"
#include "Chat.h"
#include "Block.h"
#include "Random.h"
#include "Event.h"
#include "AsyncDownloader.h"
#include "Funcs.h"
#include "Entity.h"
#include "Gui.h"
#include "Screens.h"
#include "MapGenerator.h"
#include "World.h"
#include "Camera.h"
#include "TexturePack.h"
#include "Menus.h"
#include "ErrorHandler.h"
#include "PacketHandlers.h"
#include "Inventory.h"
#include "Platform.h"

/*########################################################################################################################*
*-----------------------------------------------------Common handlers-----------------------------------------------------*
*#########################################################################################################################*/
UInt8 ServerConnection_ServerNameBuffer[String_BufferSize(STRING_SIZE)];
String ServerConnection_ServerName = String_FromEmptyArray(ServerConnection_ServerNameBuffer);
UInt8 ServerConnection_ServerMOTDBuffer[String_BufferSize(STRING_SIZE)];
String ServerConnection_ServerMOTD = String_FromEmptyArray(ServerConnection_ServerMOTDBuffer);
UInt8 ServerConnection_AppNameBuffer[String_BufferSize(STRING_SIZE)];
String ServerConnection_AppName = String_FromEmptyArray(ServerConnection_AppNameBuffer);
Int32 ServerConnection_Ticks;

static void ServerConnection_ResetState(void) {
	ServerConnection_Disconnected = false;
	ServerConnection_SupportsExtPlayerList = false;
	ServerConnection_SupportsPlayerClick = false;
	ServerConnection_SupportsPartialMessages = false;
	ServerConnection_SupportsFullCP437 = false;
}

void ServerConnection_RetrieveTexturePack(STRING_PURE String* url) {
	if (!TextureCache_HasAccepted(url) && !TextureCache_HasDenied(url)) {
		Screen* warning = TexPackOverlay_MakeInstance(url);
		Gui_ShowOverlay(warning, false);
	} else {
		ServerConnection_DownloadTexturePack(url);
	}
}

void ServerConnection_DownloadTexturePack(STRING_PURE String* url) {
	if (TextureCache_HasDenied(url)) return;
	UInt8 etagBuffer[String_BufferSize(STRING_SIZE)];
	String etag = String_InitAndClearArray(etagBuffer);
	DateTime lastModified = { 0 };

	if (TextureCache_HasUrl(url)) {
		TextureCache_GetLastModified(url, &lastModified);
		TextureCache_GetETag(url, &etag);
	}
	TexturePack_ExtractCurrent(url);

	String zip = String_FromConst(".zip");
	if (String_ContainsString(url, &zip)) {
		String texPack = String_FromConst("texturePack");
		AsyncDownloader_GetDataEx(url, true, &texPack, &lastModified, &etag);
	} else {
		String terrain = String_FromConst("terrain");
		AsyncDownloader_GetImageEx(url, true, &terrain, &lastModified, &etag);
	}
}

void ServerConnection_LogResourceFail(AsyncRequest* item) {
	Int32 status = item->StatusCode;
	if (status == 0 || status == 304) return;

	UInt8 msgBuffer[String_BufferSize(STRING_SIZE)];
	String msg = String_InitAndClearArray(msgBuffer);
	String_Format1(&msg, "&c%i error when trying to download texture pack", &status);
	Chat_Add(&msg);
}

void ServerConnection_CheckAsyncResources(void) {
	AsyncRequest item;
	String terrain = String_FromConst("terrain");
	String texPack = String_FromConst("texturePack");

	if (AsyncDownloader_Get(&terrain, &item)) {
		if (item.ResultBitmap.Scan0 != NULL) {
			TexturePack_ExtractTerrainPng_Req(&item);
		} else {
			ServerConnection_LogResourceFail(&item);
		}
	}

	if (AsyncDownloader_Get(&texPack, &item)) {
		if (item.ResultData.Ptr != NULL) {
			TexturePack_ExtractTexturePack_Req(&item);
		} else {
			ServerConnection_LogResourceFail(&item);
		}
	}
}


/*########################################################################################################################*
*--------------------------------------------------------PingList---------------------------------------------------------*
*#########################################################################################################################*/
typedef struct PingEntry_ {
	Int64 TimeSent, TimeReceived;
	UInt16 Data;
} PingEntry;
PingEntry PingList_Entries[10];

UInt16 PingList_Set(Int32 i, UInt16 prev) {
	DateTime now; Platform_CurrentUTCTime(&now);
	PingList_Entries[i].Data = (UInt16)(prev + 1);
	PingList_Entries[i].TimeSent = DateTime_TotalMs(&now);
	PingList_Entries[i].TimeReceived = 0;
	return (UInt16)(prev + 1);
}

UInt16 PingList_NextPingData(void) {
	/* Find free ping slot */
	Int32 i;
	for (i = 0; i < Array_Elems(PingList_Entries); i++) {
		if (PingList_Entries[i].TimeSent != 0) continue;
		UInt16 prev = i > 0 ? PingList_Entries[i - 1].Data : 0;
		return PingList_Set(i, prev);
	}

	/* Remove oldest ping slot */
	for (i = 0; i < Array_Elems(PingList_Entries) - 1; i++) {
		PingList_Entries[i] = PingList_Entries[i + 1];
	}
	Int32 j = Array_Elems(PingList_Entries) - 1;
	return PingList_Set(j, PingList_Entries[j].Data);
}

void PingList_Update(UInt16 data) {
	Int32 i;
	for (i = 0; i < Array_Elems(PingList_Entries); i++) {
		if (PingList_Entries[i].Data != data) continue;
		DateTime now; Platform_CurrentUTCTime(&now);
		PingList_Entries[i].TimeReceived = DateTime_TotalMs(&now);
		return;
	}
}

Int32 PingList_AveragePingMs(void) {
	Real64 totalMs = 0.0;
	Int32 measures = 0;
	Int32 i;
	for (i = 0; i < Array_Elems(PingList_Entries); i++) {
		PingEntry entry = PingList_Entries[i];
		if (entry.TimeSent == 0 || entry.TimeReceived == 0) continue;

		/* Half, because received->reply time is actually twice time it takes to send data */
		totalMs += (entry.TimeReceived - entry.TimeSent) * 0.5;
		measures++;
	}
	return measures == 0 ? 0 : (Int32)(totalMs / measures);
}


/*########################################################################################################################*
*-------------------------------------------------Singleplayer connection-------------------------------------------------*
*#########################################################################################################################*/
static void SPConnection_BeginConnect(void) {
	String logName = String_FromConst("Singleplayer");
	Chat_SetLogName(&logName);
	Game_UseCPEBlocks = Game_UseCPE;

	Int32 i, count = Game_UseCPEBlocks ? BLOCK_CPE_COUNT : BLOCK_ORIGINAL_COUNT;
	for (i = 1; i < count; i++) {
		Block_CanPlace[i]  = true;
		Block_CanDelete[i] = true;
	}
	Event_RaiseVoid(&BlockEvents_PermissionsChanged);

	/* For when user drops a map file onto ClassicalSharp.exe */
	String path = Game_Username;
	if (String_IndexOf(&path, Platform_DirectorySeparator, 0) >= 0 && Platform_FileExists(&path)) {
		LoadLevelScreen_LoadMap(&path);
		Gui_ReplaceActive(NULL);
		return;
	}

	Random rnd; Random_InitFromCurrentTime(&rnd);
	Get_SetDimensions(128, 64, 128); Gen_Vanilla = true;
	Gen_Seed = Random_Next(&rnd, Int32_MaxValue);
	Gui_ReplaceActive(GeneratingScreen_MakeInstance());
}

UInt8 SPConnection_LastCol = NULL;
static void SPConnection_AddPortion(STRING_PURE String* text) {
	UInt8 tmpBuffer[String_BufferSize(STRING_SIZE * 2)];
	String tmp = String_InitAndClearArray(tmpBuffer);
	/* Prepend colour codes for subsequent lines of multi-line chat */
	if (!Drawer2D_IsWhiteCol(SPConnection_LastCol)) {
		String_Append(&tmp, '&');
		String_Append(&tmp, SPConnection_LastCol);
	}
	String_AppendString(&tmp, text);

	Int32 i;
	/* Replace all % with & */
	for (i = 0; i < tmp.length; i++) {
		if (tmp.buffer[i] == '%') tmp.buffer[i] = '&';
	}
	String_UNSAFE_TrimEnd(&tmp);

	UInt8 col = Drawer2D_LastCol(&tmp, tmp.length);
	if (col != NULL) SPConnection_LastCol = col;
	Chat_Add(&tmp);
}

static void SPConnection_SendChat(STRING_PURE String* text) {
	if (text->length == 0) return;
	SPConnection_LastCol = NULL;

	String part = *text;
	while (part.length > STRING_SIZE) {
		String portion = String_UNSAFE_Substring(&part, 0, STRING_SIZE);
		SPConnection_AddPortion(&portion);
		part = String_UNSAFE_SubstringAt(&part, STRING_SIZE);
	}
	SPConnection_AddPortion(&part);
}

static void SPConnection_SendPosition(Vector3 pos, Real32 rotY, Real32 headX) { }
static void SPConnection_SendPlayerClick(MouseButton button, bool isDown, EntityID targetId, PickedPos* pos) { }

static void SPConnection_Tick(ScheduledTask* task) {
	if (ServerConnection_Disconnected) return;
	if ((ServerConnection_Ticks % 3) == 0) {
		Physics_Tick();
		ServerConnection_CheckAsyncResources();
	}
	ServerConnection_Ticks++;
}

void ServerConnection_InitSingleplayer(void) {
	ServerConnection_ResetState();
	Physics_Init();
	ServerConnection_SupportsFullCP437 = !Game_ClassicMode;
	ServerConnection_SupportsPartialMessages = true;
	ServerConnection_IsSinglePlayer = true;

	ServerConnection_BeginConnect = SPConnection_BeginConnect;
	ServerConnection_SendChat = SPConnection_SendChat;
	ServerConnection_SendPosition = SPConnection_SendPosition;
	ServerConnection_SendPlayerClick = SPConnection_SendPlayerClick;
	ServerConnection_Tick = SPConnection_Tick;

	ServerConnection_ReadStream  = NULL;
	ServerConnection_WriteStream = NULL;
}


/*########################################################################################################################*
*--------------------------------------------------Multiplayer connection-------------------------------------------------*
*#########################################################################################################################*/
void* net_socket;
Stream net_readStream;
Stream net_writeStream;
UInt8 net_readBuffer[4096 * 5];
UInt8 net_writeBuffer[131];

Int32 net_maxHandledPacket;
bool net_writeFailed;
Int32 net_ticks;
DateTime net_lastPacket;
UInt8 net_lastOpcode;
Real64 net_discAccumulator;

bool net_connecting;
Int64 net_connectTimeout;
#define NET_TIMEOUT_MS (15 * 1000)

static void MPConnection_BlockChanged(void* obj, Vector3I coords, BlockID oldBlock, BlockID block) {
	Vector3I p = coords;
	if (block == BLOCK_AIR) {
		block = Inventory_SelectedBlock;
		Classic_WriteSetBlock(&net_writeStream, p.X, p.Y, p.Z, false, block);
	} else {
		Classic_WriteSetBlock(&net_writeStream, p.X, p.Y, p.Z, true, block);
	}
	Net_SendPacket();
}

static void ServerConnection_Free(void);
static void MPConnection_FinishConnect(void) {
	net_connecting = false;
	Event_RaiseReal(&WorldEvents_Loading, 0.0f);

	String streamName = String_FromConst("network socket");
	Stream_ReadonlyMemory(&net_readStream, net_readBuffer, sizeof(net_readBuffer), &streamName);
	Stream_WriteonlyMemory(&net_writeStream, net_writeBuffer, sizeof(net_writeBuffer), &streamName);

	net_readStream.Meta_Mem_Left   = 0; /* initally no memory to read */
	net_readStream.Meta_Mem_Length = 0;

	Handlers_Reset();
	Classic_WriteLogin(&net_writeStream, &Game_Username, &Game_Mppass);
	Net_SendPacket();
	Platform_CurrentUTCTime(&net_lastPacket);
}

static void MPConnection_FailConnect(ReturnCode result) {
	net_connecting = false;
	UInt8 msgBuffer[String_BufferSize(STRING_SIZE * 2)];
	String msg = String_InitAndClearArray(msgBuffer);

	if (result != 0) {
		String_Format3(&msg, "Error connecting to %s:%i: %i", &Game_IPAddress, &Game_Port, &result);
		ErrorHandler_Log(&msg);
		String_Clear(&msg);
	}

	String_Format2(&msg, "Failed to connect to %s:%i", &Game_IPAddress, &Game_Port);
	String reason = String_FromConst("You failed to connect to the server. It's probably down!");
	Game_Disconnect(&msg, &reason);

	ServerConnection_Free();
}

static void MPConnection_TickConnect(void) {
	bool poll_error = false;
	Platform_SocketSelect(net_socket, SOCKET_SELECT_ERROR, &poll_error);
	if (poll_error) {
		ReturnCode err = 0; Platform_SocketGetError(net_socket, &err);
		MPConnection_FailConnect(err);
		return;
	}

	DateTime now; Platform_CurrentUTCTime(&now);
	Int64 nowMS = DateTime_TotalMs(&now);

	bool poll_write = false;
	Platform_SocketSelect(net_socket, SOCKET_SELECT_WRITE, &poll_write);

	if (poll_write) {
		Platform_SocketSetBlocking(net_socket, true);
		MPConnection_FinishConnect();
	} else if (nowMS > net_connectTimeout) {
		MPConnection_FailConnect(0);
	} else {
		Int64 leftMS = net_connectTimeout - nowMS;
		Event_RaiseReal(&WorldEvents_Loading, (Real32)leftMS / NET_TIMEOUT_MS);
	}
}

static void MPConnection_BeginConnect(void) {
	Platform_SocketCreate(&net_socket);
	Event_RegisterBlock(&UserEvents_BlockChanged, NULL, MPConnection_BlockChanged);
	ServerConnection_Disconnected = false;

	Platform_SocketSetBlocking(net_socket, false);
	net_connecting = true;
	DateTime now; Platform_CurrentUTCTime(&now);
	net_connectTimeout = DateTime_TotalMs(&now) + NET_TIMEOUT_MS;

	ReturnCode result = Platform_SocketConnect(net_socket, &Game_IPAddress, Game_Port);
	if (result == 0) return;
	if (result != ReturnCode_SocketInProgess && result != ReturnCode_SocketWouldBlock) {
		MPConnection_FailConnect(result);
	}
}

static void MPConnection_SendChat(STRING_PURE String* text) {
	if (text->length == 0 || net_connecting) return;
	String remaining = *text;

	while (remaining.length > STRING_SIZE) {
		String portion = String_UNSAFE_Substring(&remaining, 0, STRING_SIZE);
		Classic_WriteChat(&net_writeStream, &portion, true);
		Net_SendPacket();
		remaining = String_UNSAFE_SubstringAt(&remaining, STRING_SIZE);
	}

	Classic_WriteChat(&net_writeStream, &remaining, false);
	Net_SendPacket();
}

static void MPConnection_SendPosition(Vector3 pos, Real32 rotY, Real32 headX) {
	Classic_WritePosition(&net_writeStream, pos, rotY, headX);
	Net_SendPacket();
}

static void MPConnection_SendPlayerClick(MouseButton button, bool buttonDown, EntityID targetId, PickedPos* pos) {
	CPE_WritePlayerClick(&net_writeStream, button, buttonDown, targetId, pos);
	Net_SendPacket();
}

static void MPConnection_CheckDisconnection(Real64 delta) {
	net_discAccumulator += delta;
	if (net_discAccumulator < 1.0) return;
	net_discAccumulator = 0.0;

	UInt32 available = 0; bool poll_success = false;
	ReturnCode availResult  = Platform_SocketAvailable(net_socket, &available);
	ReturnCode selectResult = Platform_SocketSelect(net_socket, SOCKET_SELECT_READ, &poll_success);

	if (net_writeFailed || availResult != 0 || selectResult != 0 || (available == 0 && poll_success)) {
		String title  = String_FromConst("Disconnected!");
		String reason = String_FromConst("You've lost connection to the server");
		Game_Disconnect(&title, &reason);
	}
}

static void MPConnection_Tick(ScheduledTask* task) {
	if (ServerConnection_Disconnected) return;
	if (net_connecting) { MPConnection_TickConnect(); return; }

	DateTime now; Platform_CurrentUTCTime(&now);
	if (DateTime_MsBetween(&net_lastPacket, &now) >= 30 * 1000) {
		MPConnection_CheckDisconnection(task->Interval);
	}
	if (ServerConnection_Disconnected) return;

	UInt32 modified = 0;
	ReturnCode recvResult = Platform_SocketAvailable(net_socket, &modified);
	if (recvResult == 0 && modified > 0) {
		/* NOTE: Always using a read call that is a multiple of 4096 (appears to?) improve read performance */
		UInt8* src = net_readBuffer + net_readStream.Meta_Mem_Left;
		recvResult = Platform_SocketRead(net_socket, src, 4096 * 4, &modified);
		net_readStream.Meta_Mem_Left   += modified;
		net_readStream.Meta_Mem_Length += modified;
	}

	if (recvResult != 0) {
		UInt8 msgBuffer[String_BufferSize(STRING_SIZE * 2)];
		String msg = String_InitAndClearArray(msgBuffer);
		String_Format3(&msg, "Error reading from %s:%i: %i", &Game_IPAddress, &Game_Port, &recvResult);
		ErrorHandler_Log(&msg);

		String title  = String_FromConst("&eLost connection to the server");
		String reason = String_FromConst("I/O error when reading packets");
		Game_Disconnect(&title, &reason);
		return;
	}

	while (net_readStream.Meta_Mem_Left > 0) {
		UInt8 opcode = net_readStream.Meta_Mem_Cur[0];
		/* Workaround for older D3 servers which wrote one byte too many for HackControl packets */
		if (cpe_needD3Fix && net_lastOpcode == OPCODE_CPE_HACK_CONTROL && (opcode == 0x00 || opcode == 0xFF)) {
			Platform_LogConst("Skipping invalid HackControl byte from D3 server");
			Stream_Skip(&net_readStream, 1);

			LocalPlayer* p = &LocalPlayer_Instance;
			p->Physics.JumpVel = 0.42f; /* assume default jump height */
			p->Physics.ServerJumpVel = p->Physics.JumpVel;
			continue;
		}

		if (opcode > net_maxHandledPacket) { ErrorHandler_Fail("Invalid opcode"); }
		if (net_readStream.Meta_Mem_Left < Net_PacketSizes[opcode]) break;

		Stream_Skip(&net_readStream, 1); /* remove opcode */
		net_lastOpcode = opcode;
		Net_Handler handler = Net_Handlers[opcode];
		Platform_CurrentUTCTime(&net_lastPacket);

		if (handler == NULL) { ErrorHandler_Fail("Unsupported opcode"); }
		handler(&net_readStream);
	}

	/* Keep last few unprocessed bytes, don't care about rest since they'll be overwritten on socket read */
	Int32 i;
	for (i = 0; i < net_readStream.Meta_Mem_Left; i++) {
		net_readBuffer[i] = net_readStream.Meta_Mem_Cur[i];
	}
	net_readStream.Meta_Mem_Cur    = net_readStream.Meta_Mem_Base;
	net_readStream.Meta_Mem_Length = net_readStream.Meta_Mem_Left;

	/* Network is ticked 60 times a second. We only send position updates 20 times a second */
	if ((net_ticks % 3) == 0) {
		ServerConnection_CheckAsyncResources();
		Handlers_Tick();
		/* Have any packets been written? */
		if (net_writeStream.Meta_Mem_Cur != net_writeStream.Meta_Mem_Base) {
			Net_SendPacket();
		}
	}
	net_ticks++;
}

void Net_Set(UInt8 opcode, Net_Handler handler, UInt16 packetSize) {
	Net_Handlers[opcode] = handler;
	Net_PacketSizes[opcode] = packetSize;
	net_maxHandledPacket = max(opcode, net_maxHandledPacket);
}

void Net_SendPacket(void) {
	if (!ServerConnection_Disconnected) {
		/* NOTE: Not immediately disconnecting here, as otherwise we sometimes miss out on kick messages */
		UInt32 count = (UInt32)(net_writeStream.Meta_Mem_Cur - net_writeStream.Meta_Mem_Base), modified = 0;

		while (count > 0) {
			ReturnCode result = Platform_SocketWrite(net_socket, net_writeBuffer, count, &modified);
			if (result != 0 || modified == 0) { net_writeFailed = true; break; }
			count -= modified;
		}
	}
	
	net_writeStream.Meta_Mem_Cur   = net_writeStream.Meta_Mem_Base;
	net_writeStream.Meta_Mem_Left = net_writeStream.Meta_Mem_Length;
}

static Stream* MPConnection_ReadStream(void)  { return &net_readStream; }
static Stream* MPConnection_WriteStream(void) { return &net_writeStream; }
void ServerConnection_InitMultiplayer(void) {
	ServerConnection_ResetState();
	ServerConnection_IsSinglePlayer = false;

	ServerConnection_BeginConnect = MPConnection_BeginConnect;
	ServerConnection_SendChat = MPConnection_SendChat;
	ServerConnection_SendPosition = MPConnection_SendPosition;
	ServerConnection_SendPlayerClick = MPConnection_SendPlayerClick;
	ServerConnection_Tick = MPConnection_Tick;

	ServerConnection_ReadStream  = MPConnection_ReadStream;
	ServerConnection_WriteStream = MPConnection_WriteStream;
}


static void MPConnection_OnNewMap(void) {
	if (ServerConnection_IsSinglePlayer) return;
	/* wipe all existing entity states */
	Int32 i;
	for (i = 0; i < ENTITIES_MAX_COUNT; i++) {
		Handlers_RemoveEntity((EntityID)i);
	}
}

static void MPConnection_Reset(void) {
	if (ServerConnection_IsSinglePlayer) return;
	Int32 i;
	for (i = 0; i < OPCODE_COUNT; i++) {
		Net_Handlers[i] = NULL;
		Net_PacketSizes[i] = 0;
	}

	net_writeFailed = false;
	net_maxHandledPacket = 0;
	Handlers_Reset();
	ServerConnection_Free();
}

static void ServerConnection_Free(void) {
	if (ServerConnection_IsSinglePlayer) {
		Physics_Free();
	} else {
		if (ServerConnection_Disconnected) return;
		Event_UnregisterBlock(&UserEvents_BlockChanged, NULL, MPConnection_BlockChanged);
		Platform_SocketClose(net_socket);
		ServerConnection_Disconnected = true;
	}
}

IGameComponent ServerConnection_MakeComponent(void) {
	IGameComponent comp = IGameComponent_MakeEmpty();
	comp.OnNewMap = MPConnection_OnNewMap;
	comp.Reset    = MPConnection_Reset;
	comp.Free     = ServerConnection_Free;
	return comp;
}
