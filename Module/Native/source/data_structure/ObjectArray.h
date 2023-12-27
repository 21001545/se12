//
//  ObjectArray.h
//  NavigationModule
//
//  Created by Romanus on 2018. 4. 2..
//  Copyright 짤 2018??Romanus. All rights reserved.
//

#ifndef ObjectArray_h
#define ObjectArray_h

template<typename T>
class ObjectArray
{
private:
	T**	_array;
	int	_array_length;
	int _count;
	
public:
	inline ObjectArray()
	: _count(0), _array_length(2)
	{
		_array = new T*[2];
	}
	
	inline ObjectArray(int initial_capacity)
	{
		_array = new T*[ initial_capacity];
		_array_length = initial_capacity;
		_count = 0;
	}
	
	inline ~ObjectArray()
	{
		if( _array != NULL)
		{
			delete [] _array;
		}
	}
	
	inline int count()
	{
		return _count;
	}
	
	inline T** array()
	{
		return _array;
	}
	
	inline void add(T* item)
	{
		ensureCapacity( _count + 1);
		
		_array[ _count] = item;
		_count ++;
	}
	
	inline void clear()
	{
		_count = 0;
	}
	
	inline void ensureCapacity(int capacity)
	{
		if( _array_length < capacity)
		{
			
			int new_length = capacity * 2;
			T** new_array = new T*[new_length];

			//Logger::log( 0, "inflate array buffer %d->%d", _array_length, new_length);

			memcpy( new_array, _array, sizeof(T*)*_count);
			delete[] _array;

			_array = new_array;
			_array_length = new_length;
		}
	}
};

#endif /* ObjectArray_h */
