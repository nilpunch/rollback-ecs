using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ECS
{
	/// <summary>
	/// Contains
	/// </summary>
	public class TypesStorage
	{
		private readonly Dictionary<Type, EcsTypeInfo> _typeInfoByType;
		private readonly Dictionary<EcsId, EcsTypeInfo> _typeInfoById;

		public TypesStorage()
		{
			_typeInfoByType = new Dictionary<Type, EcsTypeInfo>();
			_typeInfoById = new Dictionary<EcsId, EcsTypeInfo>();
		}

		public EcsTypeInfo RegisterTypeWithId<T>(EcsId ecsId) where T : unmanaged
		{
			var type = typeof(T);

			var typeInfo = new EcsTypeInfo(ecsId, Marshal.SizeOf(type), HasAnyFields(type));
			_typeInfoByType.Add(type, typeInfo);
			_typeInfoById.Add(typeInfo.Id, typeInfo);

			return typeInfo;
		}

		public bool TryGetTypeInfo<T>(out EcsTypeInfo info) where T : unmanaged
		{
			return _typeInfoByType.TryGetValue(typeof(T), out info);
		}

		public bool IsRegistered<T>() where T : unmanaged
		{
			return _typeInfoByType.ContainsKey(typeof(T));
		}

		public bool IsRegistered(EcsId id)
		{
			return _typeInfoById.ContainsKey(id);
		}

		public EcsTypeInfo GetTypeInfo(EcsId id)
		{
			return _typeInfoById[id];
		}

		public IEnumerable<EcsId> GetOnlyComponentIds(IEnumerable<EcsId> ids)
		{
			foreach (var ecsId in ids)
			{
				if (_typeInfoById.TryGetValue(ecsId, out var typeInfo) && typeInfo.HasFields)
				{
					yield return ecsId;
				}
			}
		}

		private bool HasAnyFields(Type type)
		{
			return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length > 0;
		}
	}
}