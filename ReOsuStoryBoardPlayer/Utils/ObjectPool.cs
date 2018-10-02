using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Utils
{
    public class ObjectPool<T> where T : class , new()
    {
        private ConcurrentBag<T> _objects;

        private ObjectPool()
        {
            _objects = new ConcurrentBag<T>();
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

        private static ObjectPool<T> _instance;

        public static ObjectPool<T> Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ObjectPool<T>();
                return _instance;
            }
        }
    }
}
