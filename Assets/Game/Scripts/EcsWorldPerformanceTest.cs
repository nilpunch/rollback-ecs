using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ECS
{
	public struct Position
	{
		public Vector3 Value;
	}
	
	public struct Enemy
	{
	}
	
	public struct Player
	{
	}
	
	public class EcsWorldPerformanceTest : MonoBehaviour
	{
		[SerializeField] private int _entitiesAmount = 1000;
		[SerializeField] private int _framesPerFrame = 240;
		
		private World _ecs;
		private Query<Position> _enemies;
		
		private void Start()
		{
			_ecs = new World();

			_enemies = new Query<Position>(_ecs,
				include: new SortedSet<EcsId>() {_ecs.GetComponentId<Enemy>() },
				exclude: new SortedSet<EcsId>() {_ecs.GetComponentId<Player>() });

			var playerEntity = _ecs.CreateEntity();
			_ecs.Set(playerEntity, new Position());
			_ecs.Set(playerEntity, new Player());

			for (int i = 0; i < _entitiesAmount; i++)
			{
				var enemyEntity = _ecs.CreateEntity();
				_ecs.Set(enemyEntity, new Position());
				_ecs.Set(enemyEntity, new Enemy());
			}
			
			var allEntities = new Query<Position>(_ecs,
				include: new SortedSet<EcsId>(),
				exclude: new SortedSet<EcsId>());
			
			var playerEntities = new Query<Position>(_ecs,
				include: new SortedSet<EcsId>() {_ecs.GetComponentId<Player>() },
				exclude: new SortedSet<EcsId>() {_ecs.GetComponentId<Enemy>() });
			
			Debug.Log("All entities count: " + allEntities.EntitiesCount);
			Debug.Log("Enemy entities count: " + _enemies.EntitiesCount);
			Debug.Log("Player entities count: " + playerEntities.EntitiesCount);
		}

		private void Update()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			
			for (int i = 0; i < _framesPerFrame; i++)
			{
				_enemies.ForEach((ref Position position) =>
				{
					position.Value += Vector3.forward * 0.001f;
					position.Value += Vector3.forward * 0.001f;
					position.Value += Vector3.forward * 0.001f;
				});
			}

			long milliseconds = stopwatch.ElapsedMilliseconds;
			
			Debug.Log(milliseconds + "ms");
		}
	}
}