using UnityEngine;

namespace ECS
{
	public struct Health
	{
		public int Amount;
	}

	public struct Armor
	{
		public int Defense;
	}

	public struct Tag
	{
	}

	public class UnityWorldTest : MonoBehaviour
	{
		private void Start()
		{
			World world = new World();

			var entity = world.CreateEntity();

			Debug.Log("has health = " + world.Has<Health>(entity));

			world.Set(entity, new Health() { Amount = 10 });

			Debug.Log("has health = " + world.Has<Health>(entity));

			Debug.Log("health = " + world.GetComponent<Health>(entity).Amount);

			Debug.Log("has tag = " + world.Has<Tag>(entity));

			world.Set(entity, new Tag());

			Debug.Log("has tag = " + world.Has<Tag>(entity));

			world.Set(entity, new Health() { Amount = 9 });

			Debug.Log("health = " + world.GetComponent<Health>(entity).Amount);

			world.Set(entity, new Armor() { Defense = 2 });

			Debug.Log("health = " + world.GetComponent<Health>(entity).Amount);
			Debug.Log("defense = " + world.GetComponent<Armor>(entity).Defense);

			world.Remove<Health>(entity);

			Debug.Log("has health = " + world.Has<Health>(entity));

			world.Remove<Tag>(entity);

			Debug.Log("has tag = " + world.Has<Tag>(entity));

			var entities = new[]
			{
				world.CreateEntity(),
				world.CreateEntity(),
				world.CreateEntity(),
				world.CreateEntity(),
				world.CreateEntity(),
				world.CreateEntity(),
				world.CreateEntity(),
			};

			foreach (var ent in entities)
			{
				Debug.Log("ent id + gen = " + ent.Index + "_" + ent.Generation);
			}

			foreach (var ent in entities)
			{
				world.Set(ent, new Armor() { Defense = (int)ent.Index });
			}

			Debug.Log("defense = " + world.GetComponent<Armor>(entity).Defense);

			foreach (var ent in entities)
			{
				Debug.Log("ent defense = " + world.GetComponent<Armor>(ent).Defense);
				world.DestroyEntity(ent);
			}

			entities = new[]
			{
				world.CreateEntity(),
				world.CreateEntity(),
				world.CreateEntity(),
				world.CreateEntity(),
				world.CreateEntity(),
				world.CreateEntity(),
				world.CreateEntity(),
			};

			foreach (var ent in entities)
			{
				Debug.Log("ent id + gen = " + ent.Index + "_" + ent.Generation);
			}
		}
	}
}