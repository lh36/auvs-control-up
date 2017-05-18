﻿using System;

namespace MonitorNet
{
	public class Singleton<T> where T : new() 
	{
		protected static T instance = default (T);
		public static T Instance{
			get{
				if(instance == null){
					instance = new T ();
				}
				return instance;
			}
		}
	}
}

