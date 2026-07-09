using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mvvm.Editor
{
    public enum MvvmBindingMode
    {
        OneWay,
        TwoWay,
        Command
    }

    [Serializable]
    public sealed class MvvmBindingEntry
    {
        public bool enabled = true;
        public string objectPath;
        public string componentType;
        public string targetProperty;
        public MvvmBindingMode mode;
        public string viewModelMember;
        public string valueType;
        public string fieldName;
    }

    public sealed class MvvmBindingProfile : ScriptableObject
    {
        public GameObject targetPrefab;
        public string namespaceName = "Game.UI";
        public string viewClassName = "GeneratedView";
        public string viewModelClassName = "GeneratedViewModel";
        public string outputFolder = "Assets/mvvm/Generated";
        public List<MvvmBindingEntry> bindings = new List<MvvmBindingEntry>();
    }
}
