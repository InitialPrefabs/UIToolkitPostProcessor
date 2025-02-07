using System;

namespace InitialPrefabs.UIToolkit.PostProcessor {

    /// <summary>
    /// Alternative to nullables, similar to Rust.
    /// </summary>
    /// <typeparam name="T">Any type</typeparam>
    public readonly struct Option<T> {
        public bool IsValid => Value != null;

        public readonly T Value;

        public Option(T value) {
            Value = value;
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
    }
}
