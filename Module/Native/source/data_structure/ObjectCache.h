//
//  ObjectCache.hpp
//  NavigationModule
//
//  Created by Romanus on 2018. 4. 2..
//  Copyright 짤 2018??Romanus. All rights reserved.
//

#ifndef ObjectCache_h
#define ObjectCache_h

class Context;

template<typename T>
class ObjectCache
{
private:
	//Context* _context;
	T*	_free;
	int	_new_id;
	T**	_alloc_list;
	int	_alloc_capacity;
	int _count;
	
public:
	inline ObjectCache(){}
	inline ObjectCache(int initial_capacity)
	{
		init( /*context,*/ initial_capacity);
	}
	
	void init(int initial_capacity)
	{
		//_context = context;
		_new_id = 0;
		_free = NULL;
		_count = 0;
		_alloc_list = NULL;

		ensureCapacity( initial_capacity);
	}
	
	inline ~ObjectCache()
	{
		if( _alloc_list != NULL)
		{
			for(int i = 0; i < _count; ++i)
			{
				delete _alloc_list[ i];
			}

			delete [] _alloc_list;
		}
	}
	
	inline T* pop() {
		T* item;
		
		if( _free != NULL)
		{
			item = _free;
			_free = _free->_next;
		}
		else
		{
			return createOne();
		}
		
		return item;
	}
	inline void push(T* item) {
		item->_next = _free;
		_free = item;
	}
	
	inline T* fromID(int instance_id)
	{
		return _alloc_list[ instance_id];
	}
	
	inline void pushList(T** array,int count)
	{
		for(int i = 0; i < count; ++i)
		{
			push( array[ i]);
		}
	}
	
private:
	inline T* createOne()
	{
		int instance_id = _new_id;
		
		ensureCapacity(_count + 1);

		T* new_item = new T();
		new_item->initInstance( /*_context,*/ instance_id);
		
		_alloc_list[ instance_id] = new_item;
		_new_id++;
		_count++;

		return new_item;
	}
	
	inline void ensureCapacity(int capacity)
	{
		if( _alloc_list == NULL)
		{
			_alloc_list = new T*[capacity];
			_alloc_capacity = capacity;
		}
		else
		{
			if( capacity <= _alloc_capacity)
			{
				return;
			}

			int new_capacity = capacity * 2;
			T** new_list = new T*[ new_capacity];
			
			memcpy( new_list, _alloc_list, sizeof(T*) * _count);
			
			delete[] _alloc_list;
			_alloc_list = new_list;
			_alloc_capacity = new_capacity;
		}
	}
	
};

#endif /* ObjectCache_h */
