using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ECS
{
	public class TypesIdStorage
	{
		private readonly EcsIdGenerator _ecsIdGenerator;
		private readonly Dictionary<Type, EcsTypeInfo> _typeInfoByType;
		private readonly Dictionary<EcsId, EcsTypeInfo> _typeInfoById;

		public TypesIdStorage(EcsIdGenerator ecsIdGenerator)
		{
			_ecsIdGenerator = ecsIdGenerator;
			_typeInfoByType = new Dictionary<Type, EcsTypeInfo>();
			_typeInfoById = new Dictionary<EcsId, EcsTypeInfo>();
		}

		public EcsTypeInfo EnsureRegistered<T>() where T : unmanaged
		{
			var type = typeof(T);

			if (!_typeInfoByType.TryGetValue(type, out var typeInfo))
			{
				typeInfo = new EcsTypeInfo(_ecsIdGenerator.ReserveNotRecycledId(), Marshal.SizeOf(type), HasAnyFields(type));
				_typeInfoByType.Add(type, typeInfo);
				_typeInfoById.Add(typeInfo.Id, typeInfo);
			}

			return typeInfo;
		}

		public EcsTypeInfo GetTypeInfo(EcsId id)
		{
			return _typeInfoById[id];
		}

		public EcsId GetId<T>() where T : unmanaged
		{
			return _typeInfoByType[typeof(T)].Id;
		}
		
		public int GetSizeOf<T>() where T : unmanaged
		{
			return _typeInfoByType[typeof(T)].SizeOfElement;
		}

		private bool HasAnyFields(Type type)
		{
			return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length > 0;
		}
	}
}