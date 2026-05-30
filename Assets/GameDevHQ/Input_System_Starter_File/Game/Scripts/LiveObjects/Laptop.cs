using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Laptop : MonoBehaviour
    {
        [SerializeField]
        private Slider _progressBar;
        [SerializeField]
        private int _hackTime = 5;
        private bool _hacked = false;
        private bool _entered = false;
        [SerializeField]
        private CinemachineVirtualCamera[] _cameras;
        private int _activeCamera = 0;
        [SerializeField]
        private InteractableZone _interactableZone;
        private GameInput _input;

        public static event Action onHackComplete;
        public static event Action onHackEnded;

        private void OnEnable()
        {
            InteractableZone.onHoldStarted += InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded += InteractableZone_onHoldEnded;
        }

        private void Start()
        {
            InitialzeGameInput();
        }

        private void InitialzeGameInput()
        {
            _input = new GameInput();
            _input.Laptop.Enable();
            _input.Laptop.NextCamera.performed += NextCamera_Performed;
            _input.Laptop.ExitLaptop.performed += ExitLaptop_Performed;
        }

        private void NextCamera_Performed(InputAction.CallbackContext context)
        {
            if (!_entered) return;

            if (_input.Laptop.NextCamera.IsPressed())
            {
                var previous = _activeCamera;
                _activeCamera++;


                if (_activeCamera >= _cameras.Length)
                    _activeCamera = 0;


                _cameras[_activeCamera].Priority = 11;
                _cameras[previous].Priority = 9;
            }
        }

        private void ExitLaptop_Performed(InputAction.CallbackContext context)
        {
            if (!_entered) return;

            if (_input.Laptop.ExitLaptop.IsPressed())
            {
                _entered = false;
                onHackEnded?.Invoke();
                ResetCameras();
            }
        }

        void ResetCameras()
        {
            foreach (var cam in _cameras)
            {
                cam.Priority = 9;
            }
        }

        private void InteractableZone_onHoldStarted(int zoneID)
        {
            if (zoneID == 3 && _entered == false) //Hacking terminal
            {
                _progressBar.gameObject.SetActive(true);
                StartCoroutine(HackingRoutine());
                onHackComplete?.Invoke();
            }
        }

        private void InteractableZone_onHoldEnded(int zoneID)
        {
            if (zoneID == 3) //Hacking terminal
            {
                if (_hacked == true)
                    return;

                StopAllCoroutines();
                _progressBar.gameObject.SetActive(false);
                _progressBar.value = 0;
                onHackEnded?.Invoke();
            }
        }


        IEnumerator HackingRoutine()
        {
            while (_progressBar.value < 1)
            {
                _progressBar.value += Time.deltaTime / _hackTime;
                yield return new WaitForEndOfFrame();
            }

            //enter laptop mode
            _entered = true;

            //successfully hacked
            if (_hacked == false)
            {
                _hacked = true;
                _interactableZone.CompleteTask(3);
            }

            //hide progress bar
            _progressBar.gameObject.SetActive(false);

            //enable Vcam1
            _cameras[0].Priority = 11;
        }
        
        private void OnDisable()
        {
            InteractableZone.onHoldStarted -= InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded -= InteractableZone_onHoldEnded;
        }
    }

}

