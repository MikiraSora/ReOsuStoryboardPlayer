using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ReOsuStoryboardPlayer.Core.Utils
{
    public abstract class ObjectPool
    {
        public abstract void Clean();
    }

    public class ObjectPool<T> : ObjectPool where T : class, new()
    {
        private ConcurrentBag<T> _objects = new ConcurrentBag<T>();

        private static ConcurrentBag<ObjectPool> _pool_objects = new ConcurrentBag<ObjectPool>();
        private static Timer _t;

        static ObjectPool()
        {
            _t=new Timer(_ =>
          {
              foreach (var x in _pool_objects)
                  x.Clean();
          }, null, 0, 10_000);
        }

        public T GetObject()
        {
            T item;
            if (_objects.TryTake(out item)) return item;
            return new T();
        }

        public void PutObject(T item)
        {
            if (item!=null)
                _objects.Add(item);
        }

        public void PutObjects(IEnumerable<T> items)
        {
            foreach (var item in items)
                PutObject(item);
        }

        public override void Clean()
        {
            while (_objects.Count!=0)
                _objects.TryTake(out _);
        }

        private static ObjectPool<T> _instance;

        public static ObjectPool<T> Instance
        {
            get
            {
                if (_instance==null)
                {
                    _instance=new ObjectPool<T>();
                    _pool_objects.Add(_instance);
                }
                return _instance;
            }
        }
    }
}