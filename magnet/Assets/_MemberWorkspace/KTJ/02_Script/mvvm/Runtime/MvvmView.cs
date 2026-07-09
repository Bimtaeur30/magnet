using System;
using UnityEngine;

namespace Mvvm
{
    public abstract class MvvmView<TViewModel> : MonoBehaviour where TViewModel : class
    {
        private bool isBound;

        public TViewModel ViewModel { get; private set; }

        protected virtual void Awake()
        {
            if (ViewModel == null)
            {
                SetViewModel(CreateViewModel());
            }
        }

        protected virtual TViewModel CreateViewModel()
        {
            return Activator.CreateInstance<TViewModel>();
        }

        public void SetViewModel(TViewModel viewModel)
        {
            if (isBound)
            {
                OnUnbind();
                isBound = false;
            }

            ViewModel = viewModel;

            if (isActiveAndEnabled && ViewModel != null)
            {
                OnBind();
                isBound = true;
            }
        }

        protected virtual void OnEnable()
        {
            if (!isBound && ViewModel != null)
            {
                OnBind();
                isBound = true;
            }
        }

        protected virtual void OnDisable()
        {
            if (isBound)
            {
                OnUnbind();
                isBound = false;
            }
        }

        protected abstract void OnBind();
        protected abstract void OnUnbind();
    }
}
