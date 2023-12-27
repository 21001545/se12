//
//  ObjectLinkedList.h
//  NavigationModule
//
//  Created by Romanus on 2018. 4. 2..
//  Copyright 짤 2018??Romanus. All rights reserved.
//

#ifndef ObjectLinkedList_h
#define ObjectLinkedList_h

template<typename T>
class ObjectLinkedList
{
private:
	T*		_head;
	T*		_tail;
	int		_count;
	
public:
	inline ObjectLinkedList()
	{
		_head = _tail = NULL;
		_count = 0;
	}
	
	inline int	count()
	{
		return _count;
	}
	
	inline T*	head()
	{
		return _head;
	}
	
	inline T*	tail()
	{
		return _tail;
	}

	inline void push_back(T* item)
	{
		if( _head == NULL && _tail == NULL)
		{
			_head = _tail = item;
			item->_ll_prev = item->_ll_next = NULL;
		}
		else
		{
			item->_ll_prev = _tail;
			item->_ll_next = NULL;
			
			_tail->_ll_next = item;
			_tail = item;
		}
		
		++_count;
	}
	
	inline void remove(T* item)
	{
		if( item->_ll_prev != NULL)
		{
			item->_ll_prev->_ll_next = item->_ll_next;
		}
		
		if( item->_ll_next != NULL)
		{
			item->_ll_next->_ll_prev = item->_ll_prev;
		}
		
		if( _head == item && _tail == item)
		{
			_head = _tail = NULL;
		}
		else
		{
			if( _head == item)
			{
				_head = item->_ll_next;
			}
			
			if( _tail == item)
			{
				_tail = item->_ll_prev;
			}
		}
		
		--_count;
	}
};

#endif /* ObjectLinkedList_h */
