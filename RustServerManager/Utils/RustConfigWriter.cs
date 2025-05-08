using RustServerManager.ViewModels;

namespace RustServerManager.Utils
{

    public static class RustConfigWriter
    {
        public static void Save(RustInstanceEditViewModel vm, RustInstanceGridItemViewModel selected)
        {
            vm.ApplyTo(selected.Instance);
        }
    }
}

