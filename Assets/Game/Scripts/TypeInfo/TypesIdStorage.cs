using System;
using System.Collections.Generic;
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

		public void Register<T>() where T : unmanaged
		{
			var typeInfo = new EcsTypeInfo(_ecsIdGenerator.ReserveId(), Marshal.SizeOf<T>());
			_typeInfoByType.Add(typeof(T), typeInfo);
			_typeInfoById.Add(typeInfo.Id, typeInfo);
		}

		public EcsTypeInfo GetTypeInfo(EcsId id)
		{
			return _typeInfoById[id];
		}
		
		public EcsTypeInfo GetTypeInfo<T>() where T : unmanaged
		{
			return _typeInfoByType[typeof(T)];
		}
		
		public EcsId GetId<T>() where T : unmanaged
		{
			return _typeInfoByType[typeof(T)].Id;
		}
		
		public int GetSizeOf<T>() where T : unmanaged
		{
			return _typeInfoByType[typeof(T)].SizeOfElement;
		}
	}
}