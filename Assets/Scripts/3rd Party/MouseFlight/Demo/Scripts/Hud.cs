//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;

namespace MFlight.Demo
{
    public class Hud : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private MouseFlightController mouseFlight = null;

        [Header("HUD Elements")]
        [SerializeField] private RectTransform boresight = null;
        [SerializeField] private RectTransform mousePos = null;

        private Camera playerCam = null;

        private void Awake()
        {
            if (mouseFlight == null)
                Debug.LogError(name + ": Hud - Mouse Flight Controller not assigned!");

            playerCam = Camera.main;

          
        }

        private void Update()
        {
            if (mouseFlight == null || playerCam == null)
                return;

            UpdateGraphics(mouseFlight);
        }

        private void UpdateGraphics(MouseFlightController controller)
        {
            if (boresight != null)
            {
                boresight.position = playerCam.WorldToScreenPoint(controller.BoresightPos);
                boresight.gameObject.SetActive(boresight.position.z > 1f);
            }

            if (mousePos != null)
            {
                mousePos.position = playerCam.WorldToScreenPoint(controller.MouseAimPos);
                mousePos.gameObject.SetActive(mousePos.position.z > 1f);
            }
        }

        public void SetReferenceMouseFlight(MouseFlightController controller)
        {
            mouseFlight = controller;
        }
    }
}
