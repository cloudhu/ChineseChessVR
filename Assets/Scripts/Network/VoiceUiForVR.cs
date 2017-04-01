// --------------------------------------------------------------------------------------------------------------------
// <copyright file=VoiceUiForVR.cs company=League of HTC Vive Developers>
/*
11111111111111111111111111111111111111001111111111111111111111111
11111111111111111111111111111111111100011111111111111111111111111
11111111111111111111111111111111100001111111111111111111111111111
11111111111111111111111111111110000111111111111111111111111111111
11111111111111111111111111111000000111111111111111111111111111111
11111111111111111111111111100000011110001100000000000000011111111
11111111111111111100000000000000000000000000000000011111111111111
11111111111111110111000000000000000000000000000011111111111111111
11111111111111111111111000000000000000000000000000000000111111111
11111111111111111110000000000000000000000000000000111111111111111
11111111111111111100011100000000000000000000000000000111111111111
11111111111111100000110000000000011000000000000000000011111111111
11111111111111000000000000000100111100000000000001100000111111111
11111111110000000000000000001110111110000000000000111000011111111
11111111000000000000000000011111111100000000000000011110001111111
11111110000000011111111111111111111100000000000000001111100111111
11111111000001111111111111111111110000000000000000001111111111111
11111111110111111111111111111100000000000000000000000111111111111
11111111111111110000000000000000000000000000000000000111111111111
11111111111111111100000000000000000000000000001100000111111111111
11111111111111000000000000000000000000000000111100000111111111111
11111111111000000000000000000000000000000001111110000111111111111
11111111100000000000000000000000000000001111111110000111111111111
11111110000000000000000000000000000000111111111110000111111111111
11111100000000000000000001110000001111111111111110001111111111111
11111000000000000000011111111111111111111111111110011111111111111
11110000000000000001111111111111111100111111111111111111111111111
11100000000000000011111111111111111111100001111111111111111111111
11100000000001000111111111111111111111111000001111111111111111111
11000000000001100111111111111111111111111110000000111111111111111
11000000000000111011111111111100011111000011100000001111111111111
11000000000000011111111111111111000111110000000000000011111111111
11000000000000000011111111111111000000000000000000000000111111111
11001000000000000000001111111110000000000000000000000000001111111
11100110000000000001111111110000000000000000111000000000000111111
11110110000000000000000000000000000000000111111111110000000011111
11111110000000000000000000000000000000001111111111111100000001111
11111110000010000000000000000001100000000111011111111110000001111
11111111000111110000000000000111110000000000111111111110110000111
11111110001111111100010000000001111100000111111111111111110000111
11111110001111111111111110000000111111100000000111111111111000111
11111111001111111111111111111000000111111111111111111111111100011
11111111101111111111111111111110000111111111111111111111111001111
11111111111111111111111111111110001111111111111111111111100111111
11111111111111111111111111111111001111111111111111111111001111111
11111111111111111111111111111111100111111111111111111111111111111
11111111111111111111111111111111110111111111111111111111111111111
*/
//   
// </copyright>
// <summary>
//  Chinese Chess VR
// </summary>
// <author>胡良云（CloudHu）</author>
//中文注释：胡良云（CloudHu） 3/31/2017
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using Client=ExitGames.Client;

/// <summary>
/// FileName: VoiceUiForVR.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 
/// DateTime: 3/31/2017
/// </summary>
public class VoiceUiForVR : MonoBehaviour {
	
	#region Public Variables  //公共变量区域
	public bool DebugMode {
		get {
			return debugMode;
		}
		set {
			debugMode = value;
			debugGO.SetActive(debugMode);
			voiceDebugText.text = "";
			if (debugMode)
			{
				previousDebugLevel = PhotonVoiceNetwork.Client.loadBalancingPeer.DebugOut;
				PhotonVoiceNetwork.Client.loadBalancingPeer.DebugOut = Client.Photon.DebugLevel.ALL;
			} else
			{
				PhotonVoiceNetwork.Client.loadBalancingPeer.DebugOut = previousDebugLevel;
			}
			if (DebugToggled != null) {
				DebugToggled(debugMode);
			}
		}
	}

	public delegate void OnDebugToggle(bool debugMode);

	public static event OnDebugToggle DebugToggled;
	
	#endregion


	#region Private Variables   //私有变量区域
	[SerializeField]
	private Text punState;
	[SerializeField]
	private Text voiceState;

	private Canvas canvas;

	[SerializeField]
	private Button punSwitch;
	private Text punSwitchText;
	[SerializeField]
	private Button voiceSwitch;
	private Text voiceSwitchText;
	[SerializeField]
	private Button calibrateButton;
	private Text calibrateText;

	[SerializeField]
	private Text voiceDebugText;

	private PhotonVoiceRecorder rec;

	[SerializeField]
	private GameObject inGameSettings;

	[SerializeField]
	private GameObject globalSettings;

	[SerializeField]
	private Text devicesInfoText;

	private GameObject debugGO;

	private bool debugMode;

	private float volumeBeforeMute;

	private DebugLevel previousDebugLevel;

	[SerializeField]
	private int calibrationMilliSeconds = 2000;

	#endregion
	
	
	#region MonoBehaviour CallBacks //回调函数区域

	private void OnEnable() {

		GameManager.CharacterInstantiated += CharacterInstantiation_CharacterInstantiated;
		BetterToggleVR.ToggleValueChanged += BetterToggle_ToggleValueChanged;
	}

	private void OnDisable() {
		GameManager.CharacterInstantiated -= CharacterInstantiation_CharacterInstantiated;
		BetterToggleVR.ToggleValueChanged -= BetterToggle_ToggleValueChanged;
	}

	private void Start()
	{
		canvas = GetComponentInChildren<Canvas>();
		if (punSwitch != null)
		{
			punSwitchText = punSwitch.GetComponentInChildren<Text>();
			punSwitch.onClick.AddListener(PunSwitchOnClick);
		}
		if (voiceSwitch)
		{
			voiceSwitchText = voiceSwitch.GetComponentInChildren<Text>();
			voiceSwitch.onClick.AddListener(VoiceSwitchOnClick);
		}
		if (calibrateButton != null)
		{
			calibrateButton.onClick.AddListener(CalibrateButtonOnClick);
			calibrateText = calibrateButton.GetComponentInChildren<Text>();
		}
		if (punState != null)
		{
			debugGO = punState.transform.parent.gameObject;
		}
		volumeBeforeMute = AudioListener.volume;
		previousDebugLevel = PhotonVoiceNetwork.Client.loadBalancingPeer.DebugOut;
		if (globalSettings != null)
		{
			globalSettings.SetActive(true);
			InitToggles(globalSettings.GetComponentsInChildren<Toggle>());
		}
		if (devicesInfoText != null)
		{
			if (Microphone.devices == null || Microphone.devices.Length == 0)
			{
				devicesInfoText.enabled = true;
				devicesInfoText.color = Color.red;
				devicesInfoText.text = "No microphone device detected!";
			}
			else if (Microphone.devices.Length == 1)
			{
				devicesInfoText.text = string.Format("Mic.: {0}", Microphone.devices[0]);
			}
			else
			{
				devicesInfoText.text = string.Format("Multi.Mic.Devices:\n0. {0} (active)\n", Microphone.devices[0]);
				for (int i = 1; i < Microphone.devices.Length; i++)
				{
					devicesInfoText.text = string.Concat(devicesInfoText.text, string.Format("{0}. {1}\n", i, Microphone.devices[i]));
				}
			}
		}
	}
	
	private void Update() {
		// editor only two-ways binding for toggles
		#if UNITY_EDITOR
		InitToggles(globalSettings.GetComponentsInChildren<Toggle>());
		#endif
		switch (PhotonNetwork.connectionStateDetailed) {
		case ClientState.PeerCreated:
		case ClientState.Disconnected:
			punSwitch.interactable = true;
			punSwitchText.text = "PUN Connect";
			if (rec != null)
			{
				rec.enabled = false;
				rec = null;
			}
			break;
		case ClientState.Joined:
			punSwitch.interactable = true;
			punSwitchText.text = "PUN Disconnect";
			break;
		default:
			punSwitch.interactable = false;
			punSwitchText.text = "PUN busy";
			break;
		}
		switch (PhotonVoiceNetwork.ClientState) {
		case Client.Photon.LoadBalancing.ClientState.Joined:
			voiceSwitch.interactable = true;
			voiceSwitchText.text = "Voice Disconnect";
			inGameSettings.SetActive(true);
			InitToggles(inGameSettings.GetComponentsInChildren<Toggle>());
			if (rec != null) {
				calibrateButton.interactable = !rec.VoiceDetectorCalibrating;
				calibrateText.text = rec.VoiceDetectorCalibrating ? "Calibrating" : string.Format("Calibrate ({0}s)", calibrationMilliSeconds / 1000);
			}
			else {
				calibrateButton.interactable = false;
				calibrateText.text = "Unavailable";
			}
			break;
		case Client.Photon.LoadBalancing.ClientState.Uninitialized:
		case Client.Photon.LoadBalancing.ClientState.Disconnected:
			if (PhotonNetwork.inRoom)
			{
				voiceSwitch.interactable = true;
				voiceSwitchText.text = "Voice Connect";
				voiceDebugText.text = "";
			} else
			{
				voiceSwitch.interactable = false;
				voiceSwitchText.text = "Voice N/A";
				voiceDebugText.text = "";
			}
			calibrateButton.interactable = false;
			calibrateText.text = "Unavailable";
			inGameSettings.SetActive(false);
			break;
		default:
			voiceSwitch.interactable = false;
			voiceSwitchText.text = "Voice busy";
			break;
		}
		if (debugMode) {
			punState.text = string.Format("PUN: {0}", PhotonNetwork.connectionStateDetailed);
			voiceState.text = string.Format("PhotonVoice: {0}", PhotonVoiceNetwork.ClientState);
			if (rec != null && rec.LevelMeter != null) {
				voiceDebugText.text = string.Format("Amp: avg. {0}, peak {1}",
					rec.LevelMeter.CurrentAvgAmp.ToString("0.000000"),
					rec.LevelMeter.CurrentPeakAmp.ToString("0.000000"));
			} 
		}
	}

	#endregion
	
	#region Public Methods	//公共方法区域
	
	
	#endregion
	
	#region Private Methods	//私有方法区域
	private void InitToggles(Toggle[] toggles) {
		if (toggles == null) { return; }
		for (int i = 0; i < toggles.Length; i++) {
			Toggle toggle = toggles[i];
			switch (toggle.name) {
			case "Mute":
				toggle.isOn = (AudioListener.volume <= 0.001f);
				break;

			case "AutoTransmit":
				toggle.isOn = PhotonVoiceSettings.Instance.AutoTransmit;
				break;

			case "VoiceDetection":
				toggle.isOn = PhotonVoiceSettings.Instance.VoiceDetection;
				break;

			case "AutoConnect":
				toggle.isOn = PhotonVoiceSettings.Instance.AutoConnect;
				break;

			case "AutoDisconnect":
				toggle.isOn = PhotonVoiceSettings.Instance.AutoDisconnect;
				break;

			case "DebugVoice":
				DebugMode = PhotonVoiceSettings.Instance.DebugInfo;
				toggle.isOn = DebugMode;
				break;

			case "Transmit":
				toggle.isOn = (rec != null && rec.Transmit);
				break;

			case "DebugEcho":
				toggle.isOn = PhotonVoiceNetwork.Client.DebugEchoMode;
				break;

			}
		}
	}

	private void CharacterInstantiation_CharacterInstantiated(GameObject character) {
		rec = character.GetComponent<PhotonVoiceRecorder>();
		rec.enabled = true;
	}

	private void BetterToggle_ToggleValueChanged(Toggle toggle) {
		switch (toggle.name) {
		case "Mute":
			//AudioListener.pause = toggle.isOn;
			if (toggle.isOn)
			{
				volumeBeforeMute = AudioListener.volume;
				AudioListener.volume = 0f;
			}
			else
			{
				AudioListener.volume = volumeBeforeMute;
				volumeBeforeMute = 0f;
			}
			break;
		case "Transmit":
			if (rec) {
				rec.Transmit = toggle.isOn;
			}
			break;
		case "VoiceDetection":
			PhotonVoiceSettings.Instance.VoiceDetection = toggle.isOn;
			if (rec) {
				rec.Detect = toggle.isOn;
			}
			break;
		case "DebugEcho":
			PhotonVoiceNetwork.Client.DebugEchoMode = toggle.isOn;
			break;
		case "AutoConnect":
			PhotonVoiceSettings.Instance.AutoConnect = toggle.isOn;
			break;

		case "AutoDisconnect":
			PhotonVoiceSettings.Instance.AutoDisconnect = toggle.isOn;
			break;
		case "AutoTransmit":
			PhotonVoiceSettings.Instance.AutoTransmit = toggle.isOn;
			break;
		case "DebugVoice":
			DebugMode = toggle.isOn;
			PhotonVoiceSettings.Instance.DebugInfo = DebugMode;
			break;
		}
	}

	private void OnCameraChanged(Camera newCamera) {
		canvas.worldCamera = newCamera;
	}



	private void PunSwitchOnClick() {
		if (PhotonNetwork.connectionStateDetailed == ClientState.Joined) {
			PhotonNetwork.Disconnect();
		}
		else if (PhotonNetwork.connectionStateDetailed == ClientState.Disconnected ||
			PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated) {
			PhotonNetwork.ConnectUsingSettings(string.Format("1.{0}", UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex));
		}
	}

	private void VoiceSwitchOnClick() {
		if (PhotonVoiceNetwork.ClientState == Client.Photon.LoadBalancing.ClientState.Joined) {
			PhotonVoiceNetwork.Disconnect();
		}
		else if (PhotonVoiceNetwork.ClientState == Client.Photon.LoadBalancing.ClientState.Uninitialized
			|| PhotonVoiceNetwork.ClientState == Client.Photon.LoadBalancing.ClientState.Disconnected) {
			PhotonVoiceNetwork.Connect();
		}
	}

	private void CalibrateButtonOnClick() {
		if (rec && !rec.VoiceDetectorCalibrating) {
			rec.VoiceDetectorCalibrate(calibrationMilliSeconds);
		}
	}


	
	#endregion
}
