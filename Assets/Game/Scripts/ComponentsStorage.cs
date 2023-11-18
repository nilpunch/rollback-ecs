using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ECS
{
	public class ComponentsStorage
	{
		private readonly TypesIdStorage _typesIdStorage;
		private readonly Dictionary<EcsId, ComponentInfo> _componentInfos;

		public ComponentsStorage(TypesIdStorage typesIdStorage)
		{
			_typesIdStorage = typesIdStorage;
			_componentInfos = new Dictionary<EcsId, ComponentInfo>();
		}

		public int GetSizeOf<T>() where T : unmanaged
		{
			return GetOrCreateInfo<T>().SizeOfElement;
		}
		
		public int GetColumnInTable<T>(TableId tableId) where T : unmanaged
		{
			return GetOrCreateInfo<T>().ColumnInTable[tableId];
		}
		
		public bool HasColumnInTable<T>(TableId tableId) where T : unmanaged
		{
			return GetOrCreateInfo<T>().ColumnInTable.ContainsKey(tableId);
		}

		public ComponentInfo GetOrCreateInfo<T>() where T : unmanaged
		{
			var typeInfo = _typesIdStorage.GetTypeInfo<T>();

			if (!_componentInfos.TryGetValue(typeInfo.Id, out var info))
			{
				info = new ComponentInfo(typeInfo.SizeOfElement);
				_componentInfos.Add(typeInfo.Id, info);
			}

			return info;
		}
		
		public ComponentInfo GetOrCreateInfo(EcsId componentId)
		{
			var typeInfo = _typesIdStorage.GetTypeInfo(componentId);

			if (!_componentInfos.TryGetValue(typeInfo.Id, out var info))
			{
				info = new ComponentInfo(typeInfo.SizeOfElement);
				_componentInfos.Add(typeInfo.Id, info);
			}

			return info;
		}
	}
}