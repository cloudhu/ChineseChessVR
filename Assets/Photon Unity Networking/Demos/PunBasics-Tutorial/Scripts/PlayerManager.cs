// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in DemoAnimator to deal with the networked player instance
// </summary>
// <author>developer@exitgames.com</author>
// 中文注释：胡良云（CloudHu）
// --------------------------------------------------------------------------------------------------------------------

#if UNITY_5 && (!UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3) || UNITY_6
#define UNITY_MIN_5_4
#endif

using UnityEngine;
using UnityEngine.EventSystems;

namespace ExitGames.Demos.DemoAnimator
{
    /// <summary>
    /// 玩家总管，处理发射输入和射线。
    /// </summary>
    public class PlayerManager : Photon.PunBehaviour, IPunObservable
    {
        #region Public Variables    //公共变量区域

        [Tooltip("玩家UI游戏对象预设")]
        public GameObject PlayerUiPrefab;

        [Tooltip("要控制的射线游戏对象")]
        public GameObject Beams;

        [Tooltip("玩家当前的体力值")]
        public float Health = 1f;

        [Tooltip("本地玩家实例。使用这个来判断本地玩家是否在场景中。")]
        public static GameObject LocalPlayerInstance;

        #endregion  

        #region Private Variables

        //当玩家发射的时候为true
        bool IsFiring;

        #endregion

        #region MonoBehaviour CallBacks


        public void Awake()
        {
            if (this.Beams == null) //如果射线为空，则报错
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> Beams Reference.", this);
            }
            else
            {
                this.Beams.SetActive(false);
            }

            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
            if (photonView.isMine)
            {
                LocalPlayerInstance = gameObject;
            }

            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        public void Start()
        {
            CameraWork _cameraWork = gameObject.GetComponent<CameraWork>();

            if (_cameraWork != null)
            {
                if (photonView.isMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> CameraWork Component on player Prefab.", this);
            }

            // Create the UI
            if (this.PlayerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(this.PlayerUiPrefab) as GameObject;
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
            }

            #if UNITY_MIN_5_4
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
            {
                this.CalledOnLevelWasLoaded(scene.buildIndex);
            };
            #endif
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// Process Inputs if local player.
        /// Show and hide the beams
        /// Watch for end of game, when local player health is 0.
        /// </summary>
        public void Update()
        {
            // we only process Inputs and check health if we are the local player
            if (photonView.isMine)
            {
                this.ProcessInputs();

                if (this.Health <= 0f)
                {
                    GameManager.Instance.LeaveRoom();
                }
            }

            if (this.Beams != null && this.IsFiring != this.Beams.GetActive())  //触发射线的激活状态
            {
                this.Beams.SetActive(this.IsFiring);
            }
        }

        /// <summary>
        /// 当碰撞器'other'进入触发器时调用的MonoBehaviour方法。
        /// 如果碰撞器是射线就会影响到玩家的体力值
        /// 注：当跳跃的同时射击，你会发现自己的射线和自身发生交互
        /// 你可以把碰撞器移动稍远一些，以防止这样的Bug或检查光束是否属于玩家。
        /// </summary>
        public void OnTriggerEnter(Collider other)
        {
            if (!photonView.isMine)
            {
                return;
            }


            // 我们只对敌人感兴趣，我们可以通过标签来区分，也可以简单地检查名称
            if (!other.name.Contains("Beam"))
            {
                return;
            }

            this.Health -= 0.1f;
        }

        /// <summary>
        /// 每一个'other'[其他的]碰撞器触摸这个触发器的时候每帧调用的MonoBehaviour方法
        /// 当射线持续触碰玩家时我们将继续影响体力值。
        /// </summary>
        /// <param name="other">Other.</param>
        public void OnTriggerStay(Collider other)
        {
            // 如果不是本地玩家，什么都不做
            if (!photonView.isMine)
            {
                return;
            }

            // 我们只对敌人感兴趣，我们可以通过标签来区分，也可以简单地检查名称
            if (!other.name.Contains("Beam"))
            {
                return;
            }

            // 当射线持续击中我们的时候我们缓慢地影响体力值，这样玩家不得不移动来防止被击杀。
            this.Health -= 0.1f*Time.deltaTime;
        }


        #if !UNITY_MIN_5_4
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
        #endif


        /// <summary>
        /// MonoBehaviour method called after a new level of index 'level' was loaded.
        /// We recreate the Player UI because it was destroy when we switched level.
        /// Also reposition the player if outside the current arena.
        /// </summary>
        /// <param name="level">Level index loaded</param>
        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }

            GameObject _uiGo = Instantiate(this.PlayerUiPrefab) as GameObject;
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 处理输入。必须只有当玩家通过该网络游戏对象(photonView.isMine == true)认证该方法才能被使用。
        /// </summary>
        void ProcessInputs()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                // we don't want to fire when we interact with UI buttons for example. IsPointerOverGameObject really means IsPointerOver*UI*GameObject
                // notice we don't use on on GetbuttonUp() few lines down, because one can mouse down, move over a UI element and release, which would lead to not lower the isFiring Flag.
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    //	return;
                }

                if (!this.IsFiring)
                {
                    this.IsFiring = true;
                }
            }

            if (Input.GetButtonUp("Fire1"))
            {
                if (this.IsFiring)
                {
                    this.IsFiring = false;
                }
            }
        }

        #endregion

        /*
        #region IPunObservable implementation

		void IPunObservable.OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.isWriting)
			{
				// We own this player: send the others our data
				stream.SendNext(IsFiring);
				stream.SendNext(Health);
			}
            else
            {
				// Network player, receive data
				this.IsFiring = (bool)stream.ReceiveNext();
				this.Health = (float)stream.ReceiveNext();
			}
		}

        #endregion
        */

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(this.IsFiring);
                stream.SendNext(this.Health);
            }
            else
            {
                // Network player, receive data
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion
    }
}