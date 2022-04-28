using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUISettingsMenu : MonoBehaviour {
		[SerializeField] GUIScript settings;
		[SerializeField] TMP_InputField inputFieldModelX;
		[SerializeField] TMP_InputField inputFieldModelY;
		[SerializeField] TMP_InputField inputFieldModelZ;

		public void ShowMenu() {
			gameObject.SetActive(true);
		}

		public void HideMenu() {
			gameObject.SetActive(false);
		}

		public void SelectModel() {
			var extensions = new [] {
				new ExtensionFilter("VRM Files", "vrm"),
				new ExtensionFilter("All Files", "*" ),
			};
			var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

			if (paths.Length > 0) {
				string filePath = paths[0];
				settings.LoadVrmModel(filePath);
			}
		}

		private bool TryParseFloat(string s, out float value) {
			return float.TryParse(
				string.IsNullOrEmpty(s) ? "0" : s,
				NumberStyles.Number,
				CultureInfo.GetCultureInfo("en-US"),
				out value
			);
		}

		public void SetModelTransform() {
			if (!TryParseFloat(inputFieldModelX.text, out float x)
			|| !TryParseFloat(inputFieldModelY.text, out float y)
			|| !TryParseFloat(inputFieldModelZ.text, out float z)) {
				return;
			}

			settings.SetModelTransform(x, y, z);
		}
	}
}