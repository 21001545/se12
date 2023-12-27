//
//  Debug.h
//  NavigationModule
//
//  Created by Romanus on 2018. 4. 3..
//  Copyright 짤 2018??Romanus. All rights reserved.
//

#ifndef Debug_h
#define Debug_h

struct ContactInfo
{
	int body_a;
	int body_b;

	real penetration;
	Vec2 normal;
	Vec2 contact_0;
	Vec2 contact_1;
	uint32 contact_count;
	real e;
	real df;
	real sf;
};

struct CellLinkInfo
{
	int	target_cell_index;
	int cost;
	int clearance;
};

struct FieldGridInfo
{
	int		width;
	int		height;
	Vec2	field_pos;
	Vec2	field_size;
	Vec2	grid_size;
	
	int		section_count;
	int		section_size;
};

struct VectorCellInfo
{
	int				cell_index;
	int				heat_map_revision;
	int				vector_revision;
	
	int				heat;
	Vec2			vector;
	bool			line_of_sight;
};

struct NotBuiltSectionInfo
{
	int		cell_index;
	int		heat;
};

#endif /* Debug_h */
