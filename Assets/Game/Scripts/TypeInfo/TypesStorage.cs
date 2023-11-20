using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ECS
{
	public class TypesStorage
	{
		private readonly EntitiesStorage _entitiesStorage;
		private readonly Dictionary<Type, EcsTypeInfo> _typeInfoByType;
		private readonly Dictionary<EcsId, EcsTypeInfo> _typeInfoById;

		public TypesStorage(EntitiesStorage entitiesStorage)
		{
			_entitiesStorage = entitiesStorage;
			_typeInfoByType = new Dictionary<Type, EcsTypeInfo>();
			_typeInfoById = new Dictionary<EcsId, EcsTypeInfo>();
		}

		public EcsTypeInfo EnsureRegistered<T>() where T : unmanaged
		{
			var type = typeof(T);

			if (!_typeInfoByType.TryGetValue(type, out var typeInfo))
			{
				typeInfo = new EcsTypeInfo(_entitiesStorage.Create(), Marshal.SizeOf(type), HasAnyFields(type));
				_typeInfoByType.Add(type, typeInfo);
				_typeInfoById.Add(typeInfo.Id, typeInfo);
			}

			return typeInfo;
		}

		public EcsTypeInfo GetTypeInfo(EcsId id)
		{
			return _typeInfoById[id];
		}

		private bool HasAnyFields(Type type)
		{
			return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length > 0;
		}
	}
}