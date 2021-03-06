#ifndef CC_TERRAINATLAS_H
#define CC_TERRAINATLAS_H
#include "Bitmap.h"
#include "2DStructs.h"
/* Represents the 2D texture atlas of terrain.png, and converted into an array of 1D textures.
   Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
*/

#define ATLAS2D_TILES_PER_ROW 16
#define ATLAS2D_ROWS_COUNT 16

Bitmap Atlas2D_Bitmap;
Int32 Atlas2D_TileSize;
void Atlas2D_UpdateState(Bitmap* bmp);
/* Creates a native texture that contains the tile at the specified index. */
GfxResourceID Atlas2D_LoadTile(TextureLoc texLoc);
void Atlas2D_Free(void);
#define Atlas2D_TileX(texLoc) ((texLoc) % ATLAS2D_TILES_PER_ROW)
#define Atlas2D_TileY(texLoc) ((texLoc) / ATLAS2D_TILES_PER_ROW)

/* The theoretical largest number of 1D atlases that a 2D atlas can be broken down into. */
#define ATLAS1D_MAX_ATLASES (ATLAS2D_TILES_PER_ROW * ATLAS2D_ROWS_COUNT)
/* The number of tiles each 1D atlas contains. */
Int32 Atlas1D_TilesPerAtlas;
/* Size of a texture V coord V for an tile in a 1D atlas. */
Real32 Atlas1D_InvTileSize;
/* Native texture ID for each 1D atlas. */
GfxResourceID Atlas1D_TexIds[ATLAS1D_MAX_ATLASES];
/* Number of 1D atlases that actually have textures / are used. */
Int32 Atlas1D_Count;
/* Retrieves the 1D texture rectangle and 1D atlas index of the given texture. */
TextureRec Atlas1D_TexRec(TextureLoc texLoc, Int32 uCount, Int32* index);
/* Returns the index of the 1D atlas within the array of 1D atlases that contains the given texture id.*/
#define Atlas1D_Index(texLoc) ((texLoc) / Atlas1D_TilesPerAtlas)
/* Returns the index of the given texture id within a 1D atlas. */
#define Atlas1D_RowId(texLoc) ((texLoc) % Atlas1D_TilesPerAtlas)

/* Updates variables and native textures for the 1D atlas array. */
void Atlas1D_UpdateState(void);
/* Returns the count of used 1D atlases. (i.e. highest used 1D atlas index + 1) */
Int32 Atlas1D_UsedAtlasesCount(void);
void Atlas1D_Free(void);
#endif