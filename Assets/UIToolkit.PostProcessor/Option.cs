using System;
using System.Threading.Tasks;

namespace InitialPrefabs.UIToolkit.PostProcessor {
    public readonly struct Option<T> {
        public bool IsValid => Value != null;

        public readonly T Value;

        public Option(T value) {
            this.Value = value;
        }

        public static Option<T> Some(T value) {
            return new Option<T>(value);
        }

        public static Option<T> None() {
            return default;
        }

        public void Ok(Action<T> process) {
            if (IsValid) {
                process.Invoke(Value);
            }
        }

        public Task OkAsync(Func<T, Task> process) {
            if (IsValid) {
                return process.Invoke(Value);
            }
            return Task.Delay(0);
        }
    }
}
