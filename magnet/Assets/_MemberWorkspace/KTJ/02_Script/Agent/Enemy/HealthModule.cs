using GGMLib.ModuleSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy
{
    public class HealthModule : MonoBehaviour, IModule
    {
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public void Initialize(ModuleOwner owner)
        {

        }
    }
}
