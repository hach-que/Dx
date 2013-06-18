using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Process4
{
    public class DTask<T>
    {
        private bool p_Completed = false;
        public T Value { get; internal set; }

        /// <summary>
        /// Whether the task has completed.
        /// </summary>
        public bool Completed
        {
            get
            {
                return this.p_Completed;
            }
            set
            {
                if (value == true && this.p_Completed == false)
                {
                    if (this.TaskComplete != null)
                        this.TaskComplete(this, new EventArgs());
                    this.p_Completed = value;
                }
                else if (this.p_Completed != value)
                    throw new InvalidOperationException("A task can't be unmarked as completed.");
            }
        }

        /// <summary>
        /// Raised when the task completes.
        /// </summary>
        internal event EventHandler TaskComplete;

        /// <summary>
        /// Creates a new instance of DTask with the specified value.
        /// </summary>
        internal DTask()
        {
            this.Completed = false;
        }

        /// <summary>
        /// Creates a new instance of DTask with the specified value.
        /// </summary>
        public DTask(T value)
        {
            this.Value = value;
            this.Completed = false;
        }

        /// <summary>
        /// Implicit conversion operator so that returning values of the desired
        /// type will correctly compile.
        /// </summary>
        static public implicit operator DTask<T>(T value)
        {
            return new DTask<T>(value);
        }
    }
}
