#include "Formats.h"
#include "World.h"
#include "Deflate.h"
#include "Block.h"
#include "Entity.h"
#include "Platform.h"
#include "ExtMath.h"
#include "ErrorHandler.h"
#include "Game.h"
#include "ServerConnection.h"
#include "Event.h"
#include "Funcs.h"

static void Map_ReadBlocks(Stream* stream) {
	World_BlocksSize = World_Width * World_Length * World_Height;
	World_Blocks = Platform_MemAlloc(World_BlocksSize, sizeof(BlockID));
	if (World_Blocks == NULL) {
		ErrorHandler_Fail("Failed to allocate memory for reading blocks array from file");
	}
	Stream_Read(stream, World_Blocks, World_BlocksSize);
}


/*########################################################################################################################*
*--------------------------------------------------MCSharp level Format---------------------------------------------------*
*#########################################################################################################################*/
#define LVL_VERSION 1874
#define LVL_CUSTOMTILE 163
#define LVL_CHUNKSIZE 16
UInt8 Lvl_table[256 - BLOCK_CPE_COUNT] = { 0, 0, 0, 0, 39, 36, 36, 10, 46, 21, 22,
22, 22, 22, 4, 0, 22, 21, 0, 22, 23, 24, 22, 26, 27, 28, 30, 31, 32, 33,
34, 35, 36, 22, 20, 49, 45, 1, 4, 0, 9, 11, 4, 19, 5, 17, 10, 49, 20, 1,
18, 12, 5, 25, 46, 44, 17, 49, 20, 1, 18, 12, 5, 25, 36, 34, 0, 9, 11, 46,
44, 0, 9, 11, 8, 10, 22, 27, 22, 8, 10, 28, 17, 49, 20, 1, 18, 12, 5, 25, 46,
44, 11, 9, 0, 9, 11, LVL_CUSTOMTILE, 0, 0, 9, 11, 0, 0, 0, 0, 0, 0, 0, 28, 22, 21,
11, 0, 0, 0, 46, 46, 10, 10, 46, 20, 41, 42, 11, 9, 0, 8, 10, 10, 8, 0, 22, 22,
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 21, 10, 0, 0, 0, 0, 0, 22, 22, 42, 3, 2, 29,
47, 0, 0, 0, 0, 0, 27, 46, 48, 24, 22, 36, 34, 8, 10, 21, 29, 22, 10, 22, 22,
41, 19, 35, 21, 29, 49, 34, 16, 41, 0, 22 };

static void Lvl_ReadCustomBlocks(Stream* stream) {
	Int32 x, y, z, i;
	UInt8 chunk[LVL_CHUNKSIZE * LVL_CHUNKSIZE * LVL_CHUNKSIZE];
	/* skip bounds checks when we know chunk is entirely inside map */
	Int32 adjWidth  = World_Width  & ~0x0F;
	Int32 adjHeight = World_Height & ~0x0F;
	Int32 adjLength = World_Length & ~0x0F;

	for (y = 0; y < World_Height; y += LVL_CHUNKSIZE) {
		for (z = 0; z < World_Length; z += LVL_CHUNKSIZE) {
			for (x = 0; x < World_Width; x += LVL_CHUNKSIZE) {
				if (Stream_TryReadByte(stream) != 1) continue;
				Stream_Read(stream, chunk, sizeof(chunk));

				Int32 baseIndex = World_Pack(x, y, z);
				if ((x + LVL_CHUNKSIZE) <= adjWidth && (y + LVL_CHUNKSIZE) <= adjHeight && (z + LVL_CHUNKSIZE) <= adjLength) {
					for (i = 0; i < sizeof(chunk); i++) {
						Int32 xx = i & 0xF, yy = (i >> 8) & 0xF, zz = (i >> 4) & 0xF;
						Int32 index = baseIndex + World_Pack(xx, yy, zz);
						World_Blocks[index] = World_Blocks[index] == LVL_CUSTOMTILE ? chunk[i] : World_Blocks[index];
					}
				} else {
					for (i = 0; i < sizeof(chunk); i++) {
						Int32 xx = i & 0xF, yy = (i >> 8) & 0xF, zz = (i >> 4) & 0xF;
						if ((x + xx) >= World_Width || (y + yy) >= World_Height || (z + zz) >= World_Length) continue;
						Int32 index = baseIndex + World_Pack(xx, yy, zz);
						World_Blocks[index] = World_Blocks[index] == LVL_CUSTOMTILE ? chunk[i] : World_Blocks[index];
					}
				}
				
			}
		}
	}
}

static void Lvl_ConvertPhysicsBlocks(void) {
	UInt8 conv[256];
	Int32 i;
	for (i = 0; i < BLOCK_CPE_COUNT; i++)
		conv[i] = (UInt8)i;
	for (i = BLOCK_CPE_COUNT; i < 256; i++)
		conv[i] = Lvl_table[i - BLOCK_CPE_COUNT];

	Int32 alignedBlocksSize = World_BlocksSize & ~3;
	/* Bulk convert 4 blocks at once */
	UInt8* blocks = World_Blocks;
	for (i = 0; i < alignedBlocksSize; i += 4) {
		*blocks = conv[*blocks]; blocks++;
		*blocks = conv[*blocks]; blocks++;
		*blocks = conv[*blocks]; blocks++;
		*blocks = conv[*blocks]; blocks++;
	}
	for (i = alignedBlocksSize; i < World_BlocksSize; i++) {
		*blocks = conv[*blocks]; blocks++;
	}
}

void Lvl_Load(Stream* stream) {
	GZipHeader gzHeader;
	GZipHeader_Init(&gzHeader);
	while (!gzHeader.Done) { GZipHeader_Read(stream, &gzHeader); }
	
	Stream compStream;
	InflateState state;
	Inflate_MakeStream(&compStream, &state, stream);

	UInt16 header = Stream_ReadU16_LE(&compStream);
	World_Width   = header == LVL_VERSION ? Stream_ReadU16_LE(&compStream) : header;
	World_Length  = Stream_ReadU16_LE(&compStream);
	World_Height  = Stream_ReadU16_LE(&compStream);

	LocalPlayer* p = &LocalPlayer_Instance;
	p->Spawn.X = (Real32)Stream_ReadU16_LE(&compStream);
	p->Spawn.Z = (Real32)Stream_ReadU16_LE(&compStream);
	p->Spawn.Y = (Real32)Stream_ReadU16_LE(&compStream);
	p->SpawnRotY  = Math_Packed2Deg(Stream_ReadU8(&compStream));
	p->SpawnHeadX = Math_Packed2Deg(Stream_ReadU8(&compStream));

	if (header == LVL_VERSION) {
		Stream_ReadU16_LE(&compStream); /* pervisit and perbuild perms */
	}
	Map_ReadBlocks(&compStream);

	Lvl_ConvertPhysicsBlocks();
	if (Stream_TryReadByte(&compStream) == 0xBD) {
		Lvl_ReadCustomBlocks(&compStream);
	}
}


/*########################################################################################################################*
*----------------------------------------------------fCraft map format----------------------------------------------------*
*#########################################################################################################################*/
#define FCM_IDENTIFIER 0x0FC2AF40UL
#define FCM_REVISION 13
static void Fcm_ReadString(Stream* stream) {
	UInt16 length = Stream_ReadU16_LE(stream);
	ReturnCode code = Stream_Skip(stream, length);
	ErrorHandler_CheckOrFail(code, "FCM import - skipping string");
}

void Fcm_Load(Stream* stream) {
	if (Stream_ReadU32_LE(stream) != FCM_IDENTIFIER) {
		ErrorHandler_Fail("Invalid identifier in .fcm file");
	}
	if (Stream_ReadU8(stream) != FCM_REVISION) {
		ErrorHandler_Fail("Invalid revision in .fcm file");
	}

	World_Width  = Stream_ReadU16_LE(stream);
	World_Length = Stream_ReadU16_LE(stream);
	World_Height = Stream_ReadU16_LE(stream);

	LocalPlayer* p = &LocalPlayer_Instance;
	p->Spawn.X = Stream_ReadI32_LE(stream) / 32.0f;
	p->Spawn.Y = Stream_ReadI32_LE(stream) / 32.0f;
	p->Spawn.Z = Stream_ReadI16_LE(stream) / 32.0f;
	p->SpawnRotY = Math_Packed2Deg(Stream_ReadU8(stream));
	p->SpawnHeadX = Math_Packed2Deg(Stream_ReadU8(stream));

	UInt8 tmp[26];
	Stream_Read(stream, tmp, 4); /* date modified */
	Stream_Read(stream, tmp, 4); /* date created */
	Stream_Read(stream, World_Uuid, sizeof(World_Uuid));
	Stream_Read(stream, tmp, 26); /* layer index */
	Int32 metaSize = Stream_ReadU32_LE(stream);

	Stream compStream;
	InflateState state;
	Inflate_MakeStream(&compStream, &state, stream);

	Int32 i;
	for (i = 0; i < metaSize; i++) {
		Fcm_ReadString(&compStream); /* Group */
		Fcm_ReadString(&compStream); /* Key */
		Fcm_ReadString(&compStream); /* Value */
	}
	Map_ReadBlocks(&compStream);
}


/*########################################################################################################################*
*---------------------------------------------------------NBTFile---------------------------------------------------------*
*#########################################################################################################################*/
enum NBT_TAG {
	NBT_TAG_END,
	NBT_TAG_INT8,
	NBT_TAG_INT16,
	NBT_TAG_INT32,
	NBT_TAG_INT64,
	NBT_TAG_REAL32,
	NBT_TAG_REAL64,
	NBT_TAG_INT8_ARRAY,
	NBT_TAG_STRING,
	NBT_TAG_LIST,
	NBT_TAG_COMPOUND,
	NBT_TAG_INT32_ARRAY,
};

#define NBT_SMALL_SIZE STRING_SIZE
struct NbtTag_;
typedef struct NbtTag_ {
	struct NbtTag_* Parent;
	UInt8  TagID;
	UInt8  NameBuffer[String_BufferSize(NBT_SMALL_SIZE)];
	UInt32 NameSize;
	UInt32 DataSize; /* size of data for arrays */

	union {
		UInt8  Value_U8;
		Int16  Value_I16;
		Int32  Value_I32;
		Int64  Value_I64;
		Real32 Value_R32;
		Real64 Value_R64;
		UInt8  DataSmall[String_BufferSize(NBT_SMALL_SIZE)];
		UInt8* DataBig; /* malloc for big byte arrays */
	};
} NbtTag;

static UInt8 NbtTag_U8(NbtTag* tag) {
	if (tag->TagID != NBT_TAG_INT8) { ErrorHandler_Fail("Expected I8 NBT tag"); }
	return tag->Value_U8;
}

static Int16 NbtTag_I16(NbtTag* tag) {
	if (tag->TagID != NBT_TAG_INT16) { ErrorHandler_Fail("Expected I16 NBT tag"); }
	return tag->Value_I16;
}

static Int32 NbtTag_I32(NbtTag* tag) {
	if (tag->TagID != NBT_TAG_INT32) { ErrorHandler_Fail("Expected I32 NBT tag"); }
	return tag->Value_I32;
}

static Int64 NbtTag_I64(NbtTag* tag) {
	if (tag->TagID != NBT_TAG_INT64) { ErrorHandler_Fail("Expected I64 NBT tag"); }
	return tag->Value_I64;
}

static Real32 NbtTag_R32(NbtTag* tag) {
	if (tag->TagID != NBT_TAG_REAL32) { ErrorHandler_Fail("Expected R32 NBT tag"); }
	return tag->Value_R32;
}

static Real64 NbtTag_R64(NbtTag* tag) {
	if (tag->TagID != NBT_TAG_REAL64) { ErrorHandler_Fail("Expected R64 NBT tag"); }
	return tag->Value_R64;
}

static UInt8 NbtTag_U8_At(NbtTag* tag, Int32 i) {
	if (tag->TagID != NBT_TAG_INT8_ARRAY) { ErrorHandler_Fail("Expected I8_Array NBT tag"); }
	if (i >= tag->DataSize) { ErrorHandler_Fail("Tried to access past bounds of I8_Array tag"); }

	if (tag->DataSize < NBT_SMALL_SIZE) return tag->DataSmall[i];
	return tag->DataBig[i];
}

static String NbtTag_String(NbtTag* tag) {
	if (tag->TagID != NBT_TAG_STRING) { ErrorHandler_Fail("Expected String NBT tag"); }
	return String_Init(tag->DataSmall, tag->DataSize, tag->DataSize);
}

static UInt32 Nbt_ReadString(Stream* stream, UInt8* strBuffer) {
	UInt16 nameLen = Stream_ReadU16_BE(stream);
	if (nameLen > NBT_SMALL_SIZE * 4) ErrorHandler_Fail("NBT String too long");
	UInt8 nameBuffer[NBT_SMALL_SIZE * 4];
	Stream_Read(stream, nameBuffer, nameLen);

	/* TODO: Check how slow reading strings this way is */
	Stream memStream; 
	Stream_ReadonlyMemory(&memStream, nameBuffer, nameLen, &stream->Name);
	UInt16 codepoint;

	UInt32 i;
	for (i = 0; i < NBT_SMALL_SIZE; i++) {
		if (!Stream_ReadUtf8Char(&memStream, &codepoint)) break;
		strBuffer[i] = Convert_UnicodeToCP437(codepoint);
	}
	return i;
}

typedef bool (*Nbt_Callback)(NbtTag* tag);
static void Nbt_ReadTag(UInt8 typeId, bool readTagName, Stream* stream, NbtTag* parent, Nbt_Callback callback) {
	if (typeId == NBT_TAG_END) return;

	NbtTag tag;
	tag.TagID = typeId;
	tag.Parent = parent;
	tag.NameSize = readTagName ? Nbt_ReadString(stream, tag.NameBuffer) : 0;
	tag.DataSize = 0;

	UInt8 childTagId;
	UInt32 i, count;

	switch (typeId) {
	case NBT_TAG_INT8:
		tag.Value_U8 = Stream_ReadU8(stream); break;
	case NBT_TAG_INT16:
		tag.Value_I16 = Stream_ReadI16_BE(stream); break;
	case NBT_TAG_INT32:
		tag.Value_I32 = Stream_ReadI32_BE(stream); break;
	case NBT_TAG_INT64:
		tag.Value_I64 = Stream_ReadI64_BE(stream); break;
	case NBT_TAG_REAL32:
		/* TODO: Is this union abuse even legal */
		tag.Value_I32 = Stream_ReadI32_BE(stream); break;
	case NBT_TAG_REAL64:
		/* TODO: Is this union abuse even legal */
		tag.Value_I64 = Stream_ReadI64_BE(stream); break;

	case NBT_TAG_INT8_ARRAY:
		count = Stream_ReadU32_BE(stream); 
		tag.DataSize = count;

		if (count < NBT_SMALL_SIZE) {
			Stream_Read(stream, tag.DataSmall, count);
		} else {
			tag.DataBig = Platform_MemAlloc(count, sizeof(UInt8));
			if (tag.DataBig == NULL) ErrorHandler_Fail("Nbt_ReadTag - allocating memory");
			Stream_Read(stream, tag.DataBig, count);
		}
		break;

	case NBT_TAG_STRING:
		tag.DataSize = Nbt_ReadString(stream, tag.DataSmall);
		break;

	case NBT_TAG_LIST:
		childTagId = Stream_ReadU8(stream);
		count = Stream_ReadU32_BE(stream);
		for (i = 0; i < count; i++) {
			Nbt_ReadTag(childTagId, false, stream, &tag, callback);
		}
		break;

	case NBT_TAG_COMPOUND:
		while ((childTagId = Stream_ReadU8(stream)) != NBT_TAG_END) {
			Nbt_ReadTag(childTagId, true, stream, &tag, callback);
		} 
		break;

	case NBT_TAG_INT32_ARRAY:
		ErrorHandler_Fail("Nbt Tag Int32_Array not supported");
		break;

	default:
		ErrorHandler_Fail("Unrecognised NBT tag");
	}

	bool processed = callback(&tag);
	/* don't leak memory for unprocessed tags */
	if (!processed && tag.DataSize >= NBT_SMALL_SIZE) Platform_MemFree(&tag.DataBig);
}

static bool IsTag(NbtTag* tag, const UInt8* tagName) {
	String name = { tag->NameBuffer, tag->NameSize, tag->NameSize };
	return String_CaselessEqualsConst(&name, tagName);
}


#define Nbt_WriteU8(stream, value)  Stream_WriteU8(stream, value)
#define Nbt_WriteI16(stream, value) Stream_WriteI16_BE(stream, value)
#define Nbt_WriteI32(stream, value) Stream_WriteI32_BE(stream, value)

static void Nbt_WriteString(Stream* stream, STRING_PURE String* text) {
	if (text->length > NBT_SMALL_SIZE) ErrorHandler_Fail("NBT String too long");
	UInt8 buffer[NBT_SMALL_SIZE * 3];
	Int32 i, len = 0;

	for (i = 0; i < text->length; i++) {
		UInt8* cur = buffer + len;
		UInt16 codepoint = Convert_CP437ToUnicode(text->buffer[i]);
		len += Stream_WriteUtf8(cur, codepoint);
	}

	Nbt_WriteI16(stream, len);
	Stream_Write(stream, buffer, len);
}

static void Nbt_WriteTag(Stream* stream, UInt8 tagType, const UInt8* tagName) {
	Nbt_WriteU8(stream, tagType);
	String str = String_FromReadonly(tagName);
	Nbt_WriteString(stream, &str);
}


/*########################################################################################################################*
*--------------------------------------------------ClassicWorld format----------------------------------------------------*
*#########################################################################################################################*/
static bool Cw_Callback_1(NbtTag* tag) {
	if (IsTag(tag, "X")) { World_Width  = (UInt16)NbtTag_I16(tag); return true; }
	if (IsTag(tag, "Y")) { World_Height = (UInt16)NbtTag_I16(tag); return true; }
	if (IsTag(tag, "Z")) { World_Length = (UInt16)NbtTag_I16(tag); return true; }

	if (IsTag(tag, "UUID")) {
		if (tag->DataSize != sizeof(World_Uuid)) ErrorHandler_Fail("Map UUID must be 16 bytes");
		Platform_MemCpy(World_Uuid, tag->DataSmall, sizeof(World_Uuid));
		return true;
	}
	if (IsTag(tag, "BlockArray")) {
		World_BlocksSize = tag->DataSize;
		if (tag->DataSize < NBT_SMALL_SIZE) {
			World_Blocks = Platform_MemAlloc(World_BlocksSize, sizeof(UInt8));
			if (World_Blocks == NULL) ErrorHandler_Fail("Failed to allocate memory for map");
			Platform_MemCpy(World_Blocks, tag->DataSmall, tag->DataSize);
		} else {
			World_Blocks = tag->DataBig;
		}
		return true;
	}
	return false;
}

static bool Cw_Callback_2(NbtTag* tag) {
	if (!IsTag(tag->Parent, "Spawn")) return false;

	LocalPlayer*p = &LocalPlayer_Instance;
	if (IsTag(tag, "X")) { p->Spawn.X = NbtTag_I16(tag); return true; }
	if (IsTag(tag, "Y")) { p->Spawn.Y = NbtTag_I16(tag); return true; }
	if (IsTag(tag, "Z")) { p->Spawn.Z = NbtTag_I16(tag); return true; }
	if (IsTag(tag, "H")) { p->SpawnRotY  = Math_Deg2Packed(NbtTag_U8(tag)); return true; }
	if (IsTag(tag, "P")) { p->SpawnHeadX = Math_Deg2Packed(NbtTag_U8(tag)); return true; }

	return false;
}

BlockID cw_curID;
Int16 cw_colR, cw_colG, cw_colB;
static PackedCol Cw_ParseCol(PackedCol defValue) {
	Int16 r = cw_colR, g = cw_colG, b = cw_colB;	
	if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255) {
		return defValue;
	} else {
		PackedCol col = PACKEDCOL_CONST((UInt8)r, (UInt8)g, (UInt8)b, 255);
		return col;		
	}
}

static bool Cw_Callback_4(NbtTag* tag) {
	if (!IsTag(tag->Parent->Parent, "CPE")) return false;
	if (!IsTag(tag->Parent->Parent->Parent, "Metadata")) return false;
	LocalPlayer*p = &LocalPlayer_Instance;

	if (IsTag(tag->Parent, "ClickDistance")) {
		if (IsTag(tag, "Distance")) { p->ReachDistance = NbtTag_I16(tag) / 32.0f; return true; }
	}
	if (IsTag(tag->Parent, "EnvWeatherType")) {
		if (IsTag(tag, "WeatherType")) { WorldEnv_SetWeather(NbtTag_U8(tag)); return true; }
	}

	if (IsTag(tag->Parent, "EnvMapAppearance")) {
		if (IsTag(tag, "SideBlock")) { WorldEnv_SetSidesBlock(NbtTag_U8(tag));  return true; }
		if (IsTag(tag, "EdgeBlock")) { WorldEnv_SetEdgeBlock(NbtTag_U8(tag));   return true; }
		if (IsTag(tag, "SideLevel")) { WorldEnv_SetEdgeHeight(NbtTag_I16(tag)); return true; }

		if (IsTag(tag, "TextureURL")) {
			String url = NbtTag_String(tag);
			if (Game_AllowServerTextures && url.length > 0) {
				ServerConnection_RetrieveTexturePack(&url);
			}
			return true;
		}
	}

	/* Callback for compound tag is called after all its children have been processed */
	if (IsTag(tag->Parent, "EnvColors")) {
		if (IsTag(tag, "Sky")) {
			WorldEnv_SetSkyCol(Cw_ParseCol(WorldEnv_DefaultSkyCol)); return true;
		} else if (IsTag(tag, "Cloud")) {
			WorldEnv_SetCloudsCol(Cw_ParseCol(WorldEnv_DefaultCloudsCol)); return true;
		} else if (IsTag(tag, "Fog")) {
			WorldEnv_SetFogCol(Cw_ParseCol(WorldEnv_DefaultFogCol)); return true;
		} else if (IsTag(tag, "Sunlight")) {
			WorldEnv_SetSunCol(Cw_ParseCol(WorldEnv_DefaultSunCol)); return true;
		} else if (IsTag(tag, "Ambient")) {
			WorldEnv_SetShadowCol(Cw_ParseCol(WorldEnv_DefaultShadowCol)); return true;
		}
	}

	if (IsTag(tag->Parent, "BlockDefinitions")) {
		String tagName = { tag->NameBuffer, tag->NameSize, tag->NameSize };
		String blockStr = String_FromConst("Block");
		if (!String_CaselessStarts(&tagName, &blockStr)) return false;
		BlockID id = cw_curID;

		/* hack for sprite draw (can't rely on order of tags when reading) */
		if (Block_SpriteOffset[id] == 0) {
			Block_SpriteOffset[id] = Block_Draw[id];
			Block_Draw[id] = DRAW_SPRITE;
		} else {
			Block_SpriteOffset[id] = 0;
		}

		Block_DefineCustom(id);
		Block_CanPlace[id]  = true;
		Block_CanDelete[id] = true;
		Event_RaiseVoid(&BlockEvents_PermissionsChanged);

		cw_curID = 0;
		return true;
	}
	return false;
}

static bool Cw_Callback_5(NbtTag* tag) {
	if (!IsTag(tag->Parent->Parent->Parent, "CPE")) return false;
	if (!IsTag(tag->Parent->Parent->Parent->Parent, "Metadata")) return false;
	LocalPlayer*p = &LocalPlayer_Instance;

	if (IsTag(tag->Parent->Parent, "EnvColors")) {
		if (IsTag(tag, "R")) { cw_colR = NbtTag_I16(tag); return true; }
		if (IsTag(tag, "G")) { cw_colG = NbtTag_I16(tag); return true; }
		if (IsTag(tag, "B")) { cw_colB = NbtTag_I16(tag); return true; }
	}

	if (IsTag(tag->Parent->Parent, "BlockDefinitions") && Game_AllowCustomBlocks) {
		BlockID id = cw_curID;
		if (IsTag(tag, "ID"))             { cw_curID = NbtTag_U8(tag); return true; }
		if (IsTag(tag, "CollideType"))    { Block_SetCollide(id, NbtTag_U8(tag)); return true; }
		if (IsTag(tag, "Speed"))          { Block_SpeedMultiplier[id] = NbtTag_R32(tag); return true; }
		if (IsTag(tag, "TransmitsLight")) { Block_BlocksLight[id] = NbtTag_U8(tag) == 0; return true; }
		if (IsTag(tag, "FullBright"))     { Block_FullBright[id] = NbtTag_U8(tag) != 0; return true; }
		if (IsTag(tag, "BlockDraw"))      { Block_Draw[id] = NbtTag_U8(tag); return true; }
		if (IsTag(tag, "Shape"))          { Block_SpriteOffset[id] = NbtTag_U8(tag); return true; }

		if (IsTag(tag, "Name")) {
			String name = NbtTag_String(tag);
			Block_SetName(id, &name);
			return true;
		}

		if (IsTag(tag, "Textures")) {
			Block_SetTex(NbtTag_U8_At(tag, 0), FACE_YMAX, id);
			Block_SetTex(NbtTag_U8_At(tag, 1), FACE_YMIN, id);
			Block_SetTex(NbtTag_U8_At(tag, 2), FACE_XMIN, id);
			Block_SetTex(NbtTag_U8_At(tag, 3), FACE_XMAX, id);
			Block_SetTex(NbtTag_U8_At(tag, 4), FACE_ZMIN, id);
			Block_SetTex(NbtTag_U8_At(tag, 5), FACE_ZMAX, id);
			return true;
		}
		
		if (IsTag(tag, "WalkSound")) {
			UInt8 sound = NbtTag_U8(tag);
			Block_DigSounds[id]  = sound;
			Block_StepSounds[id] = sound;
			if (sound == SOUND_GLASS) Block_StepSounds[id] = SOUND_STONE;
			return true;
		}

		if (IsTag(tag, "Fog")) {
			Block_FogDensity[id] = (NbtTag_U8_At(tag, 0) + 1) / 128.0f;
			/* Fix for older ClassicalSharp versions which saved wrong fog density value */
			if (NbtTag_U8_At(tag, 0) == 0xFF) Block_FogDensity[id] = 0.0f;
 
			Block_FogCol[id].R = NbtTag_U8_At(tag, 1);
			Block_FogCol[id].G = NbtTag_U8_At(tag, 2);
			Block_FogCol[id].B = NbtTag_U8_At(tag, 3);
			Block_FogCol[id].A = 255;
			return true;
		}

		if (IsTag(tag, "Coords")) {
			Block_MinBB[id].X = NbtTag_U8_At(tag, 0) / 16.0f; Block_MaxBB[id].X = NbtTag_U8_At(tag, 3) / 16.0f;
			Block_MinBB[id].Y = NbtTag_U8_At(tag, 1) / 16.0f; Block_MaxBB[id].Y = NbtTag_U8_At(tag, 4) / 16.0f;
			Block_MinBB[id].Z = NbtTag_U8_At(tag, 2) / 16.0f; Block_MaxBB[id].Z = NbtTag_U8_At(tag, 5) / 16.0f;
			return true;
		}
	}
	return false;
}

static bool Cw_Callback(NbtTag* tag) {
	UInt32 depth = 0;
	NbtTag* tmp = tag->Parent;
	while (tmp != NULL) { depth++; tmp = tmp->Parent; }

	switch (depth) {
	case 1: return Cw_Callback_1(tag);
	case 2: return Cw_Callback_2(tag);
	case 4: return Cw_Callback_4(tag);
	case 5: return Cw_Callback_5(tag);
	}
	return false;

	/* ClassicWorld -> Metadata -> CPE -> ExtName -> [values]
	        0             1         2        3          4   */
}

void Cw_Load(Stream* stream) {
	GZipHeader gzHeader;
	GZipHeader_Init(&gzHeader);
	while (!gzHeader.Done) { GZipHeader_Read(stream, &gzHeader); }

	Stream compStream;
	InflateState state;
	Inflate_MakeStream(&compStream, &state, stream);

	if (Stream_ReadU8(&compStream) != NBT_TAG_COMPOUND) {
		ErrorHandler_Fail("NBT file most start with Compound Tag");
	}
	Nbt_ReadTag(NBT_TAG_COMPOUND, true, &compStream, NULL, Cw_Callback);

	/* Older versions incorrectly multiplied spawn coords by * 32, so we check for that */
	Vector3* spawn = &LocalPlayer_Instance.Spawn;
	Vector3I P; Vector3I_Floor(&P, spawn);
	if (!World_IsValidPos_3I(P)) { spawn->X /= 32.0f; spawn->Y /= 32.0f; spawn->Z /= 32.0f; }
}


/*########################################################################################################################*
*-------------------------------------------------Minecraft .dat format---------------------------------------------------*
*#########################################################################################################################*/
enum JTypeCode {
	TC_NULL = 0x70, TC_REFERENCE = 0x71, TC_CLASSDESC = 0x72, TC_OBJECT = 0x73, 
	TC_STRING = 0x74, TC_ARRAY = 0x75, TC_ENDBLOCKDATA = 0x78,
};

enum JFieldType {
	JFIELD_INT8 = 'B', JFIELD_REAL32 = 'F', JFIELD_INT32 = 'I', JFIELD_INT64 = 'J',
	JFIELD_BOOL = 'Z', JFIELD_ARRAY = '[', JFIELD_OBJECT = 'L',
};

#define JNAME_SIZE 48
typedef struct JFieldDesc_ {
	UInt8 Type;
	UInt8 FieldName[String_BufferSize(JNAME_SIZE)];
	union {
		Int8   Value_I8;
		Int32  Value_I32;
		Int64  Value_I64;
		Real32 Value_R32;
		struct { UInt8* Value_Ptr; UInt32 Value_Size; };
	};
} JFieldDesc;

typedef struct JClassDesc_ {
	UInt8 ClassName[String_BufferSize(JNAME_SIZE)];
	UInt16 FieldsCount;
	JFieldDesc Fields[22];
} JClassDesc;

static void Dat_ReadString(Stream* stream, UInt8* buffer) {
	Platform_MemSet(buffer, 0, JNAME_SIZE);
	UInt16 len = Stream_ReadU16_BE(stream);

	if (len > JNAME_SIZE) ErrorHandler_Fail("Dat string too long");
	Stream_Read(stream, buffer, len);
}

static void Dat_ReadFieldDesc(Stream* stream, JFieldDesc* desc) {
	desc->Type = Stream_ReadU8(stream);
	Dat_ReadString(stream, desc->FieldName);

	if (desc->Type == JFIELD_ARRAY || desc->Type == JFIELD_OBJECT) {
		UInt8 typeCode = Stream_ReadU8(stream);
		if (typeCode == TC_STRING) {
			UInt8 className1[String_BufferSize(JNAME_SIZE)];
			Dat_ReadString(stream, className1);
		} else if (typeCode == TC_REFERENCE) {
			Stream_ReadI32_BE(stream); /* handle */
		} else {
			ErrorHandler_Fail("Unsupported type code in FieldDesc class name");
		}
	}
}

static void Dat_ReadClassDesc(Stream* stream, JClassDesc* desc) {
	UInt8 typeCode = Stream_ReadU8(stream);
	if (typeCode == TC_NULL) { desc->ClassName[0] = NULL; desc->FieldsCount = 0; return; }
	if (typeCode != TC_CLASSDESC) ErrorHandler_Fail("Unsupported type code in ClassDesc header");

	Dat_ReadString(stream, desc->ClassName);
	Stream_ReadU64_BE(stream); /* serial version UID */
	Stream_ReadU8(stream);     /* flags */

	desc->FieldsCount = Stream_ReadU16_BE(stream);
	if (desc->FieldsCount > Array_Elems(desc->Fields)) ErrorHandler_Fail("ClassDesc has too many fields");
	Int32 i;
	for (i = 0; i < desc->FieldsCount; i++) {
		Dat_ReadFieldDesc(stream, &desc->Fields[i]);
	}

	typeCode = Stream_ReadU8(stream);
	if (typeCode != TC_ENDBLOCKDATA) ErrorHandler_Fail("Unsupported type code in ClassDesc footer");
	JClassDesc superClassDesc;
	Dat_ReadClassDesc(stream, &superClassDesc);
}

static void Dat_ReadFieldData(Stream* stream, JFieldDesc* field) {
	switch (field->Type) {
	case JFIELD_INT8:
		field->Value_I8  = Stream_ReadI8(stream); break;
	case JFIELD_REAL32:
		/* TODO: Is this union abuse even legal */
		field->Value_I32 = Stream_ReadI32_BE(stream); break;
	case JFIELD_INT32:
		field->Value_I32 = Stream_ReadI32_BE(stream); break;
	case JFIELD_INT64:
		field->Value_I64 = Stream_ReadI64_BE(stream); break;
	case JFIELD_BOOL:
		field->Value_I32 = Stream_ReadU8(stream) != 0; break;

	case JFIELD_OBJECT: {
		/* Luckily for us, we only have to account for blockMap object */
		/* Other objects (e.g. player) are stored after the fields we actually care about, so ignore them */
		String fieldName = String_FromRawArray(field->FieldName);
		if (!String_CaselessEqualsConst(&fieldName, "blockMap")) break;

		UInt8 typeCode = Stream_ReadU8(stream);
		/* Skip all blockMap data with awful hacks */
		/* These offsets were based on server_level.dat map from original minecraft classic server */
		if (typeCode == TC_OBJECT) {
			Stream_Skip(stream, 315);
			UInt32 count = Stream_ReadU32_BE(stream);
			Stream_Skip(stream, 17 * count);
			Stream_Skip(stream, 152);
		} else if (typeCode != TC_NULL) {
			/* WoM maps have this field as null, which makes things easier for us */
			ErrorHandler_Fail("Unsupported type code in Object field");
		}
	} break;

	case JFIELD_ARRAY: {
		UInt8 typeCode = Stream_ReadU8(stream);
		if (typeCode == TC_NULL) break;
		if (typeCode != TC_ARRAY) ErrorHandler_Fail("Unsupported type code in Array field");

		JClassDesc arrayClassDesc;
		Dat_ReadClassDesc(stream, &arrayClassDesc);
		if (arrayClassDesc.ClassName[1] != JFIELD_INT8) ErrorHandler_Fail("Only byte array fields supported");

		UInt32 size = Stream_ReadU32_BE(stream);
		field->Value_Ptr = Platform_MemAlloc(size, sizeof(UInt8));
		if (field->Value_Ptr == NULL) ErrorHandler_Fail("Failed to allocate memory for map");

		Stream_Read(stream, field->Value_Ptr, size);
		field->Value_Size = size;
	} break;
	}
}

static Int32 Dat_I32(JFieldDesc* field) {
	if (field->Type != JFIELD_INT32) ErrorHandler_Fail("Field type must be Int32");
	return field->Value_I32;
}

void Dat_Load(Stream* stream) {
	GZipHeader gzHeader;
	GZipHeader_Init(&gzHeader);
	while (!gzHeader.Done) { GZipHeader_Read(stream, &gzHeader); }

	Stream compStream;
	InflateState state;
	Inflate_MakeStream(&compStream, &state, stream);

	/* .dat header */
	if (Stream_ReadI32_BE(&compStream) != 0x271BB788 || Stream_ReadU8(&compStream) != 0x02) {
		ErrorHandler_Fail("Unexpected constant in .dat file");
	}

	/* Java seralisation headers */
	if (Stream_ReadU16_BE(&compStream) != 0xACED || Stream_ReadU16_BE(&compStream) != 0x0005) {
		ErrorHandler_Fail("Unexpected java serialisation constant(s)");
	}

	UInt8 typeCode = Stream_ReadU8(&compStream);
	if (typeCode != TC_OBJECT) ErrorHandler_Fail("Unexpected type code for root class");
	JClassDesc obj; Dat_ReadClassDesc(&compStream, &obj);

	Int32 i;
	for (i = 0; i < obj.FieldsCount; i++) {
		Dat_ReadFieldData(&compStream, &obj.Fields[i]);
	}

	Vector3* spawn = &LocalPlayer_Instance.Spawn;
	for (i = 0; i < obj.FieldsCount; i++) {
		JFieldDesc* field = &obj.Fields[i];
		String fieldName = String_FromRawArray(field->FieldName);

		if (String_CaselessEqualsConst(&fieldName, "width")) {
			World_Width = Dat_I32(field);
		} else if (String_CaselessEqualsConst(&fieldName, "height")) {
			World_Length = Dat_I32(field);
		} else if (String_CaselessEqualsConst(&fieldName, "depth")) {
			World_Height = Dat_I32(field);
		} else if (String_CaselessEqualsConst(&fieldName, "blocks")) {
			if (field->Type != JFIELD_ARRAY) ErrorHandler_Fail("Blocks field must be Array");
			World_Blocks = field->Value_Ptr;
			World_BlocksSize = field->Value_Size;
		} else if (String_CaselessEqualsConst(&fieldName, "xSpawn")) {
			spawn->X = (Real32)Dat_I32(field);
		} else if (String_CaselessEqualsConst(&fieldName, "ySpawn")) {
			spawn->Y = (Real32)Dat_I32(field);
		} else if (String_CaselessEqualsConst(&fieldName, "zSpawn")) {
			spawn->Z = (Real32)Dat_I32(field);
		}
	}
}


/*########################################################################################################################*
*--------------------------------------------------ClassicWorld export----------------------------------------------------*
*#########################################################################################################################*/
static void Cw_WriteCpeExtCompound(Stream* stream, const UInt8* tagName, Int32 version) {
	Nbt_WriteTag(stream, NBT_TAG_COMPOUND, tagName);
	Nbt_WriteTag(stream, NBT_TAG_INT32, "ExtensionVersion");
	Nbt_WriteI32(stream, version);
}

static void Cw_WriteSpawnCompound(Stream* stream) {
	Nbt_WriteTag(stream, NBT_TAG_COMPOUND, "Spawn");
	LocalPlayer* p = &LocalPlayer_Instance;
	Vector3 spawn = p->Spawn; /* TODO: Maybe keep real spawn too? */

	Nbt_WriteTag(stream, NBT_TAG_INT16, "X");
	Nbt_WriteI16(stream, spawn.X);
	Nbt_WriteTag(stream, NBT_TAG_INT16, "Y");
	Nbt_WriteI16(stream, spawn.Y);
	Nbt_WriteTag(stream, NBT_TAG_INT16, "Z");
	Nbt_WriteI16(stream, spawn.Z);

	Nbt_WriteTag(stream, NBT_TAG_INT8, "H");
	Nbt_WriteU8(stream, Math_Deg2Packed(p->SpawnRotY));
	Nbt_WriteTag(stream, NBT_TAG_INT8, "P");
	Nbt_WriteU8(stream, Math_Deg2Packed(p->SpawnHeadX));

	Nbt_WriteU8(stream, NBT_TAG_END);
}

static void Cw_WriteColCompound(Stream* stream, const UInt8* tagName, PackedCol col) {
	Nbt_WriteTag(stream, NBT_TAG_COMPOUND, tagName);

	Nbt_WriteTag(stream, NBT_TAG_INT16, "R");
	Nbt_WriteI16(stream, col.R);
	Nbt_WriteTag(stream, NBT_TAG_INT16, "G");
	Nbt_WriteI16(stream, col.G);
	Nbt_WriteTag(stream, NBT_TAG_INT16, "B");
	Nbt_WriteI16(stream, col.B);

	Nbt_WriteU8(stream, NBT_TAG_END);
}

static void Cw_WriteBlockDefinitionCompound(Stream* stream, BlockID id) {
	Nbt_WriteTag(stream, NBT_TAG_COMPOUND, "Block" + id);
	bool sprite = Block_Draw[id] == DRAW_SPRITE;

	Nbt_WriteTag(stream, NBT_TAG_INT8, "ID");
	Nbt_WriteU8(stream, id);
	Nbt_WriteTag(stream, NBT_TAG_STRING, "Name");
	String name = Block_UNSAFE_GetName(id);
	Nbt_WriteString(stream, &name);

	Nbt_WriteTag(stream, NBT_TAG_INT8, "CollideType");
	Nbt_WriteU8(stream, Block_Collide[id]);
	IntAndFloat speed; speed.fVal = Block_SpeedMultiplier[id];
	Nbt_WriteTag(stream, NBT_TAG_REAL32, "Speed");
	Nbt_WriteI32(stream, speed.iVal);

	Nbt_WriteTag(stream, NBT_TAG_INT8_ARRAY, "Textures");
	Nbt_WriteI32(stream, 6);
	Nbt_WriteU8(stream, Block_GetTexLoc(id, FACE_YMAX));
	Nbt_WriteU8(stream, Block_GetTexLoc(id, FACE_YMIN));
	Nbt_WriteU8(stream, Block_GetTexLoc(id, FACE_XMIN));
	Nbt_WriteU8(stream, Block_GetTexLoc(id, FACE_XMAX));
	Nbt_WriteU8(stream, Block_GetTexLoc(id, FACE_ZMIN));
	Nbt_WriteU8(stream, Block_GetTexLoc(id, FACE_ZMAX));

	Nbt_WriteTag(stream, NBT_TAG_INT8, "TransmitsLight");
	Nbt_WriteU8(stream, Block_BlocksLight[id] ? 0 : 1);
	Nbt_WriteTag(stream, NBT_TAG_INT8, "WalkSound");
	Nbt_WriteU8(stream, Block_DigSounds[id]);
	Nbt_WriteTag(stream, NBT_TAG_INT8, "FullBright");
	Nbt_WriteU8(stream, Block_FullBright[id] ? 1 : 0);

	Nbt_WriteTag(stream, NBT_TAG_INT8, "Shape");
	UInt8 shape = sprite ? 0 : (UInt8)(Block_MaxBB[id].Y * 16);
	Nbt_WriteU8(stream, shape);
	Nbt_WriteTag(stream, NBT_TAG_INT8, "BlockDraw");
	UInt8 draw = sprite ? Block_SpriteOffset[id] : Block_Draw[id];
	Nbt_WriteU8(stream, draw);

	PackedCol col = Block_FogCol[id];
	Nbt_WriteTag(stream, NBT_TAG_INT8_ARRAY, "Fog");
	Nbt_WriteI32(stream, 4);
	UInt8 fog = (UInt8)(128 * Block_FogDensity[id] - 1);
	Nbt_WriteU8(stream, Block_FogDensity[id] == 0 ? 0 : fog);
	Nbt_WriteU8(stream, col.R); Nbt_WriteU8(stream, col.G); Nbt_WriteU8(stream, col.B);

	Vector3 minBB = Block_MinBB[id], maxBB = Block_MaxBB[id];
	Nbt_WriteTag(stream, NBT_TAG_INT8_ARRAY, "Coords");
	Nbt_WriteI32(stream, 6);
	Nbt_WriteU8(stream, minBB.X * 16); Nbt_WriteU8(stream, minBB.Y * 16);
	Nbt_WriteU8(stream, minBB.Z * 16); Nbt_WriteU8(stream, maxBB.X * 16);
	Nbt_WriteU8(stream, maxBB.Y * 16); Nbt_WriteU8(stream, maxBB.Z * 16);

	Nbt_WriteU8(stream, NBT_TAG_END);
}

static void Cw_WriteMetadataCompound(Stream* stream) {
	Nbt_WriteTag(stream, NBT_TAG_COMPOUND, "Metadata");
	Nbt_WriteTag(stream, NBT_TAG_COMPOUND, "CPE");

	Cw_WriteCpeExtCompound(stream, "ClickDistance", 1);
	Nbt_WriteTag(stream, NBT_TAG_INT16, "Distance");
	Nbt_WriteI16(stream, LocalPlayer_Instance.ReachDistance * 32);
	Nbt_WriteU8(stream, NBT_TAG_END);

	Cw_WriteCpeExtCompound(stream, "EnvWeatherType", 1);
	Nbt_WriteTag(stream, NBT_TAG_INT8, "WeatherType");
	Nbt_WriteU8(stream, WorldEnv_Weather);
	Nbt_WriteU8(stream, NBT_TAG_END);

	Cw_WriteCpeExtCompound(stream, "EnvMapAppearance", 1);
	Nbt_WriteTag(stream, NBT_TAG_INT8, "SideBlock");
	Nbt_WriteU8(stream, WorldEnv_SidesBlock);
	Nbt_WriteTag(stream, NBT_TAG_INT8, "EdgeBlock");
	Nbt_WriteU8(stream, WorldEnv_EdgeBlock);
	Nbt_WriteTag(stream, NBT_TAG_INT16, "SideLevel");
	Nbt_WriteI16(stream, WorldEnv_EdgeHeight);
	Nbt_WriteTag(stream, NBT_TAG_STRING, "TextureURL");
	Nbt_WriteString(stream, &World_TextureUrl);
	Nbt_WriteU8(stream, NBT_TAG_END);

	Cw_WriteCpeExtCompound(stream, "EnvColors", 1);
	Cw_WriteColCompound(stream, "Sky", WorldEnv_SkyCol);
	Cw_WriteColCompound(stream, "Cloud", WorldEnv_CloudsCol);
	Cw_WriteColCompound(stream, "Fog", WorldEnv_FogCol);
	Cw_WriteColCompound(stream, "Ambient", WorldEnv_ShadowCol);
	Cw_WriteColCompound(stream, "Sunlight", WorldEnv_SunCol);
	Nbt_WriteU8(stream, NBT_TAG_END);

	Cw_WriteCpeExtCompound(stream, "BlockDefinitions", 1);
	Int32 block;
	for (block = 1; block < 256; block++) {
		if (Block_IsCustomDefined((BlockID)block)) {
			Cw_WriteBlockDefinitionCompound(stream, (BlockID)block);
		}
	}
	Nbt_WriteU8(stream, NBT_TAG_END);

	Nbt_WriteU8(stream, NBT_TAG_END);
	Nbt_WriteU8(stream, NBT_TAG_END);
}

void Cw_Save(Stream* stream) {
	GZipState state;
	Stream compStream;
	GZip_MakeStream(&compStream, &state, stream);
	stream = &compStream;

	Nbt_WriteTag(stream, NBT_TAG_COMPOUND, "ClassicWorld");

	Nbt_WriteTag(stream, NBT_TAG_INT8, "FormatVersion");
	Nbt_WriteU8(stream, 1);

	Nbt_WriteTag(stream, NBT_TAG_INT8_ARRAY, "UUID");
	Nbt_WriteI32(stream, sizeof(World_Uuid));
	Stream_Write(stream, World_Uuid, sizeof(World_Uuid));

	Nbt_WriteTag(stream, NBT_TAG_INT16, "X");
	Nbt_WriteI16(stream, World_Width);
	Nbt_WriteTag(stream, NBT_TAG_INT16, "Y");
	Nbt_WriteI16(stream, World_Height);
	Nbt_WriteTag(stream, NBT_TAG_INT16, "Z");
	Nbt_WriteI16(stream, World_Length);

	Cw_WriteSpawnCompound(stream);

	Nbt_WriteTag(stream, NBT_TAG_INT8_ARRAY, "BlockArray");
	Nbt_WriteI32(stream, World_BlocksSize);
	Stream_Write(stream, World_Blocks, World_BlocksSize);

	Cw_WriteMetadataCompound(stream);

	Nbt_WriteU8(stream, NBT_TAG_END);
	stream->Close(stream);
}


/*########################################################################################################################*
*---------------------------------------------------Schematic export------------------------------------------------------*
*#########################################################################################################################*/
void Schematic_Save(Stream* stream) {
	GZipState state;
	Stream compStream;
	GZip_MakeStream(&compStream, &state, stream);
	stream = &compStream;

	Nbt_WriteTag(stream, NBT_TAG_COMPOUND, "Schematic");

	Nbt_WriteTag(stream, NBT_TAG_STRING, "Materials");
	String classic = String_FromConst("Classic");
	Nbt_WriteString(stream, &classic);

	Nbt_WriteTag(stream, NBT_TAG_INT16, "Width");
	Nbt_WriteI16(stream, World_Width);
	Nbt_WriteTag(stream, NBT_TAG_INT16, "Height");
	Nbt_WriteI16(stream, World_Height);
	Nbt_WriteTag(stream, NBT_TAG_INT16, "Length");
	Nbt_WriteI16(stream, World_Length);

	Nbt_WriteTag(stream, NBT_TAG_INT8_ARRAY, "Blocks");
	Nbt_WriteI32(stream, World_BlocksSize);
	Stream_Write(stream, World_Blocks, World_BlocksSize);

	Nbt_WriteTag(stream, NBT_TAG_INT8_ARRAY, "Data");
	Nbt_WriteI32(stream, World_BlocksSize);
	UInt8 chunk[8192] = { 0 };
	Int32 i;
	for (i = 0; i < World_BlocksSize; i += sizeof(chunk)) {
		Int32 count = min(sizeof(chunk), World_BlocksSize - i);
		Stream_Write(stream, chunk, count);
	}

	Nbt_WriteTag(stream, NBT_TAG_LIST, "Entities");
	Nbt_WriteU8(stream, NBT_TAG_COMPOUND); Nbt_WriteI32(stream, 0);
	Nbt_WriteTag(stream, NBT_TAG_LIST, "TileEntities");
	Nbt_WriteU8(stream, NBT_TAG_COMPOUND); Nbt_WriteI32(stream, 0);

	Nbt_WriteU8(stream, NBT_TAG_END);
	stream->Close(stream);
}