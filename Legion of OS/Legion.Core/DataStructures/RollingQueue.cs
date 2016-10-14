/**
 *	Copyright 2016 Dartmouth-Hitchcock
 *	
 *	Licensed under the Apache License, Version 2.0 (the "License");
 *	you may not use this file except in compliance with the License.
 *	You may obtain a copy of the License at
 *	
 *	    http://www.apache.org/licenses/LICENSE-2.0
 *	
 *	Unless required by applicable law or agreed to in writing, software
 *	distributed under the License is distributed on an "AS IS" BASIS,
 *	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *	See the License for the specific language governing permissions and
 *	limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Legion.Core.DataStructures {

    /// <summary>
    /// Queue which keeps the most recent n items.
    /// </summary>
    /// <typeparam name="T">The type of queue</typeparam>
    internal class RollingQueue<T> : Queue<T> {
        private int _size;

        /// <summary>
        /// The size of the queue
        /// </summary>
        public int Size {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Is the queue full
        /// </summary>
        public bool IsFull {
            get { return Count >= _size; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">The size of the queue</param>
        public RollingQueue(int size)
            : base(size) {
            _size = size;
        }

        /// <summary>
        /// Queue a new item and drop the oldest one if necessary
        /// </summary>
        /// <param name="item">The item to queue</param>
        new public void Enqueue(T item) {
            if (Count >= _size)
                Dequeue();

            base.Enqueue(item);
        }
    }
}
