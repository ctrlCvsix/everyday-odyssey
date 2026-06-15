using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace EverydayOdyssey
{
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(AudioSource))]
    public class BillboardAdScreen : MonoBehaviour
    {
        [SerializeField] private string videoFileName = "zhuanzhuan_ad.mp4";
        [SerializeField] private MeshRenderer screenRenderer;
        [SerializeField] private Material fallbackMaterial;
        [SerializeField] private float audibleRadius = 18f;
        [SerializeField] private bool playAudio;
        [SerializeField] private int textureWidth = 1280;
        [SerializeField] private int textureHeight = 720;

        private VideoPlayer videoPlayer;
        private AudioSource audioSource;
        private Transform player;
        private RenderTexture renderTexture;
        private Material videoMaterial;

        private void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            audioSource = GetComponent<AudioSource>();
            player = FindAnyObjectByType<PlayerController>()?.transform;

            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = audibleRadius;
            audioSource.rolloffMode = AudioRolloffMode.Linear;

            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            renderTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
            renderTexture.name = $"{name}_RenderTexture";
            renderTexture.Create();
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.audioOutputMode = playAudio ? VideoAudioOutputMode.AudioSource : VideoAudioOutputMode.None;
            if (playAudio)
            {
                videoPlayer.SetTargetAudioSource(0, audioSource);
            }

            ApplyBrightStandby();
            ConfigureVideo();
        }

        private void Update()
        {
            if (!playAudio || audioSource == null || player == null)
            {
                return;
            }

            audioSource.mute = Vector3.Distance(player.position, transform.position) > audibleRadius;
        }

        public void Configure(MeshRenderer rendererReference, Material fallbackReference, bool enableAudio)
        {
            screenRenderer = rendererReference;
            fallbackMaterial = fallbackReference;
            playAudio = enableAudio;
        }

        private void OnDestroy()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
            }
        }

        private void ConfigureVideo()
        {
            string path = Path.Combine(Application.streamingAssetsPath, videoFileName);
            if (!File.Exists(path))
            {
                ApplyFallback();
                return;
            }

            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = "file:///" + path.Replace("\\", "/");
            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.errorReceived += OnError;
            videoPlayer.Prepare();
        }

        private void OnPrepared(VideoPlayer source)
        {
            source.prepareCompleted -= OnPrepared;
            ApplyVideoMaterial();
            source.Play();
            if (playAudio && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        private void OnError(VideoPlayer source, string message)
        {
            source.errorReceived -= OnError;
            Debug.LogWarning($"Billboard video failed: {message}");
            ApplyFallback();
        }

        private void ApplyBrightStandby()
        {
            if (screenRenderer == null)
            {
                return;
            }

            Material standby = new Material(Shader.Find("Unlit/Color") ?? Shader.Find("Standard"));
            standby.color = new Color(0.1f, 0.9f, 1f);
            screenRenderer.material = standby;
        }

        private void ApplyVideoMaterial()
        {
            if (screenRenderer == null)
            {
                return;
            }

            Shader shader = Shader.Find("Unlit/Texture") ?? Shader.Find("Standard");
            videoMaterial = new Material(shader);
            videoMaterial.mainTexture = renderTexture;
            videoMaterial.color = Color.white;
            screenRenderer.material = videoMaterial;
        }

        private void ApplyFallback()
        {
            if (screenRenderer == null)
            {
                return;
            }

            screenRenderer.material = fallbackMaterial != null ? fallbackMaterial : screenRenderer.material;
        }
    }
}
