//
//  ReusableObject.hpp
//  NavigationModule
//
//  Created by Romanus on 2018. 4. 2..
//  Copyright 짤 2018??Romanus. All rights reserved.
//

#ifndef ReusableObject_hpp
#define ReusableObject_hpp

//class Context;

template<typename T>
class ResuableObject
{
public:
//	Context*	_context;
	int			_instance_id;
	T*			_next;
	
	void initInstance(int id)
	{
//		_context = context;
		_instance_id = id;
		_next = NULL;
	}
};

#endif /* ReusableObject_hpp */
