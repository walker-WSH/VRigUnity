using Mediapipe;
using Mediapipe.Unity;
using System;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	public class HolisticTrackingSolutionWithDebug : HolisticTrackingSolution {
		[Header("Debug")]
		[SerializeField] private Mediapipe.Unity.Screen screen;
		[SerializeField] private HandGroup handGroup;
		[SerializeField] private RectTransform _worldAnnotationArea;
		[SerializeField] private DetectionAnnotationController _poseDetectionAnnotationController;
		[SerializeField] private HolisticLandmarkListAnnotationController _holisticAnnotationController;
		[SerializeField] private PoseWorldLandmarkListAnnotationController _poseWorldLandmarksAnnotationController;
		[SerializeField] private NormalizedRectAnnotationController _poseRoiAnnotationController;
		
		private Groups.HandPoints handPoints = new();
		private bool hasHandData;

		// List of debug transforms
		public Transform[] debugTransforms;

		public void SetDebug(bool enable) {
			foreach (Transform t in debugTransforms) {
				t.gameObject.SetActive(enable);
			}
		}

		protected override void OnStartRun() {
			base.OnStartRun();

			graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
			graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
			graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
			graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
			graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
			graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
			graphRunner.OnPoseRoiOutput += OnPoseRoiOutput;

			var imageSource = SolutionUtils.GetImageSource();
			SetupAnnotationController(_poseDetectionAnnotationController, imageSource);
			SetupAnnotationController(_holisticAnnotationController, imageSource);
			SetupAnnotationController(_poseWorldLandmarksAnnotationController, imageSource);
			SetupAnnotationController(_poseRoiAnnotationController, imageSource);
		}

		protected override void SetupScreen(ImageSource imageSource) {
			// NOTE: Without this line the screen does not update its size and no annotations are drawn
			screen.Initialize(imageSource);
			_worldAnnotationArea.localEulerAngles = imageSource.rotation.Reverse().GetEulerAngles();
		}

		private void OnPoseDetectionOutput(object stream, OutputEventArgs<Detection> eventArgs) {
			_poseDetectionAnnotationController.DrawLater(eventArgs.value);
		}
		
		private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			_holisticAnnotationController.DrawPoseLandmarkListLater(eventArgs.value);
		}

		private void OnPoseRoiOutput(object stream, OutputEventArgs<NormalizedRect> eventArgs) {
			_poseRoiAnnotationController.DrawLater(eventArgs.value);
		}

		private void OnFaceLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			_holisticAnnotationController.DrawFaceLandmarkListLater(eventArgs.value);
		}

		private void OnLeftHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			_holisticAnnotationController.DrawLeftHandLandmarkListLater(eventArgs.value);
		}

		private void OnRightHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			_holisticAnnotationController.DrawRightHandLandmarkListLater(eventArgs.value);

			if (eventArgs.value == null) {
				return;
			}

			int count = eventArgs.value.Landmark.Count;
			for (int i = 0; i < count; i++) {
				NormalizedLandmark mark = eventArgs.value.Landmark[i];
				handPoints.Data[i] = new(mark.X * 2, -mark.Y, -mark.Z);
			}

			hasHandData = true;
		}

		private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs) {
			_poseWorldLandmarksAnnotationController.DrawLater(eventArgs.value);
		}

		new void FixedUpdate() {
			if (handGroup != null && handPoints != null) {
				handGroup.Apply(handPoints, animator.GetBoneTransform(HumanBodyBones.LeftHand).transform.position, 0.5f);
			}

			// Debug
			if (hasHandData) {
				HandResolver.SolveRightHand(handPoints);
			}

			base.FixedUpdate();
		}
	}
}