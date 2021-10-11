namespace ChangeStreamWatcher_Blazor.Services.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public delegate void OnLimitReachedCallback<T>(ICollection<T> flushedData);

    /// <summary>
    /// This class should provide a custom data structure with limited features.
    /// Its main responsibility should be to guarantee fast, multi-threaded queue. 
    /// </summary>
    public class MultiThreadedQueue<T>
    {
        private readonly object _lockState = new object();
        private readonly int _limit;

        private Node<T> _head;
        private Node<T> _tail;

        public MultiThreadedQueue(int limit = -1)
        {
            this._limit = limit;
        }

        public event OnLimitReachedCallback<T> OnLimitReached;

        public int Count { get; private set; }

        public void Enqueue(T data)
        {
            if (data is null)
                return;

            var newNode = new Node<T>(data);
            this.ExecuteConcurrently(
                () =>
                {
                    if (this.Count <= 0)
                    {
                        this._head = newNode;
                        this._tail = newNode;
                    }
                    else
                    {
                        this._tail.Next = newNode;
                        this._tail = this._tail.Next;
                    }

                    this.Count++;

                    this.TryInvokeLimitReachedCallback();
                });
        }

        public ICollection<T> FlushAllData()
        {
            ICollection<T> allData = null;

            this.ExecuteConcurrently(
                () =>
                {
                    allData = this.FlushDataInternally();
                });

            return allData;
        }

        public void Clear()
        {
            this.ExecuteConcurrently(this.ClearInternally);
        }

        private void TryInvokeLimitReachedCallback()
        {
            if (this._limit < this.Count || this.OnLimitReached is null)
                return;

            var allData = this.FlushDataInternally();
            this.OnLimitReached?.Invoke(allData);
        }

        private ICollection<T> FlushDataInternally()
        {
            var allData = new List<T>(this.Count);
            var iterationNode = this._head;
            var index = 0;
            while (iterationNode != null && index++ < this.Count)
            {
                allData.Add(iterationNode.Value);
                iterationNode = iterationNode.Next;
            }

            this.ClearInternally();
            return allData;
        }

        private void ClearInternally()
        {
            this._head = null;
            this._tail = null;
            this.Count = 0;
        }

        private void ExecuteConcurrently(Action action)
        {
            var lockTaken = false;

            try
            {
                Monitor.Enter(this._lockState, ref lockTaken);
                action();
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(this._lockState);
            }
        }
    }
}
