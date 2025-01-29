using System;

namespace InitialPrefabs.UIToolkit.PostProcessor {
    public readonly struct Option<T> {
        public bool IsValid => value != null;

        private readonly T value;

        public Option(T value) {
            this.value = value;
        }

        public static Option<T> Some(T value) {
            return new Option<T>(value);
        }

        public static Option<T> None() {
            return default;
        }

        public void Ok(Action<T> process) {
            if (IsValid) {
                process.Invoke(value);
            }
        }
    }
}
