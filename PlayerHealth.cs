using System.Collections;
using UnityEngine.UI;
using UnityEngine;

namespace Space
{
    public class PlayerHealth : MonoBehaviour
    {
        public float maxHealth = 100f;
        private float currentHealth;
        public bool _dead = false;
        public bool _invincibility = false;
        [SerializeField] private Button _tryAgain;
        [SerializeField] private Button _quit;
        [SerializeField] private GameObject _damageEffect;
        [SerializeField] private GameObject _veinEffect;
        [SerializeField] private GameObject _asciiArt;
        [SerializeField] private GameObject _gameOverCursor;
        [SerializeField] private GameObject _deathWinUI;
        [SerializeField] private GameObject _deathLight;
        [SerializeField] InteractableScreen _screen;
        [SerializeField] private GameObject _deathWinCamera;
        [SerializeField] private float buttonAppearanceDelay = 5f; // Delay in seconds before buttons appear
        [SerializeField] private TetherToSpaceship _tetherScript;
        [SerializeField] private Collider _mauriArea;

        private Image _damageImage;
        private Coroutine _damageFadeCoroutine;

        // Health regeneration variables
        public float regenDelay = 5f; // Time in seconds before regeneration starts after taking damage
        public float regenAmount = 5f; // Health points to regain per regen tick
        public float regenRate = 1f; // Time between each regen tick in seconds

        private float lastDamageTime; // Track the last time player took damage

        // Smooth saturation variables
        private float currentSaturation = 0f; // Track the current saturation level
        private float saturationSmoothSpeed = 5f; // Adjust speed for smoothing

        void Start()
        {
            _invincibility = false;
            currentHealth = maxHealth;
            _dead = false;

            _gameOverCursor.SetActive(false);
            _deathWinCamera.SetActive(false);
            _veinEffect.SetActive(false);
            _asciiArt.SetActive(false); // Disable ASCII art initially
            _tryAgain.gameObject.SetActive(false);
            _quit.gameObject.SetActive(false);

            _damageImage = _damageEffect.GetComponent<Image>();

            if (_damageImage != null)
            {
                SetImageAlpha(_damageImage, 0f); // Start invisible
                SetImageSaturation(_damageImage, 0f); // Start at no extra saturation
            }
        }

        void Update()
        {
            // Regenerate health if enough time has passed since last damage
            if (Time.time - lastDamageTime >= regenDelay && currentHealth < maxHealth && currentHealth > 0)
            {
                RegenerateHealth();
            }

            // Smoothly adjust the saturation
            float targetSaturation = 1f - Mathf.Clamp01(currentHealth / maxHealth);
            currentSaturation = Mathf.Lerp(currentSaturation, targetSaturation, Time.deltaTime * saturationSmoothSpeed);
            SetImageSaturation(_damageImage, currentSaturation);
        }

        public void TakeDamage(float damage)
        {
            if (!_invincibility || damage > maxHealth)
            {
                currentHealth -= damage;
                lastDamageTime = Time.time; // Reset the regen timer
                _veinEffect.SetActive(true);
                //PLAY DAMAGE SOUND EFFECT(s) HERE
                AudioManager.Instance.PlayAudio(GSFX._DAMAGE, transform.position, 0.5f, Random.Range(0.5f, 1.7f));

                // Trigger damage effect fade-in if not dead
                if (!_dead)
                {
                    if (_damageFadeCoroutine == null)
                    {
                        _damageFadeCoroutine = StartCoroutine(FadeIn(_damageImage)); // Start fading in
                    }
                }

                if (currentHealth <= 0)
                {
                    if (damage > maxHealth)
                    {
                        Die(true);
                    }
                    else
                    {
                        Die(false);
                    }
                }
                if (_dead && damage > maxHealth)
                {
                    StartCoroutine(EnableButtonsWithDelay());
                    _deathWinCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
                    _deathWinCamera.GetComponent<Camera>().backgroundColor = Color.black;
                    _deathLight.SetActive(false);
                }
            }
        }

        private void Die(bool instantDeath)
        {
            Player.PlayerTools.SetEnable(false);
            Player.PlayerInteractor.enabled = false;
            Player.PlayerMovement.enabled = false;
            Player.PauseMenu.enabled = false;
            _dead = true;
            _invincibility = true;
            //PLAY DEATH SOUND HERE
            AudioManager.Instance.PlayAudio(GSFX._DEATH, transform.position);

            _gameOverCursor.SetActive(true);
            _deathWinCamera.SetActive(true);
            _asciiArt.SetActive(true); // Enable ASCII art on death
            //PLAY DEATH SCREEN SOUND EFFECT HERE
            AudioManager.Instance.PlayAudio(GSFX._BLACK_BOX, transform.position);


            // Ensure damage effect stays fully saturated
            SetImageSaturation(_damageImage, 1f);

            // Enable buttons after delay
            StartCoroutine(EnableButtonsWithDelay());
            if (instantDeath)
            {
                _deathWinCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
                _deathWinCamera.GetComponent<Camera>().backgroundColor = Color.black;
                _deathLight.SetActive(false);
            }

            _screen.Enter();
            Debug.Log("Game Over");
        }

        private IEnumerator EnableButtonsWithDelay()
        {
            Debug.Log($"Button appearance delay started: {Time.time}"); // Log the start time

            // Wait for the specified delay
            yield return new WaitForSeconds(buttonAppearanceDelay);

            // Enable the buttons
            _tryAgain.gameObject.SetActive(true);
            _quit.gameObject.SetActive(true);

            Debug.Log($"Button appearance delay ended: {Time.time}");
        }

        private void RegenerateHealth()
        {
            currentHealth += regenAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            if (currentHealth == maxHealth)
            {
                if (_damageFadeCoroutine != null)
                {
                    StopCoroutine(_damageFadeCoroutine);
                    _damageFadeCoroutine = null; // Reset coroutine reference
                }
                _damageFadeCoroutine = StartCoroutine(FadeOut(_damageImage)); // Fade out when fully healed
                _veinEffect.SetActive(false);
            }

            lastDamageTime = Time.time - regenDelay + regenRate;
            Debug.Log(currentHealth);
        }

        public void TryAgain()
        {
            GameManager.TransitionToSceneChange(GameState.InGame);
        }

        private void OnTriggerStay(Collider other)
        {
            if (other == _mauriArea)
            {
                _tetherScript.enabled = false;
            }
        }

        private IEnumerator FadeIn(Image image)
        {
            yield return FadeImage(image, 0f, 1f, 0.5f); // Fade in completely
        }

        private IEnumerator FadeOut(Image image)
        {
            yield return FadeImage(image, image.color.a, 0f, 0.5f); // Fade out completely
            _damageFadeCoroutine = null; // Reset coroutine reference after fade-out
        }

        private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                SetImageAlpha(image, alpha);
                yield return null;
            }

            SetImageAlpha(image, endAlpha);
        }

        private void SetImageAlpha(Image image, float alpha)
        {
            if (image != null)
            {
                Color color = image.color;
                color.a = alpha;
                image.color = color;
            }
        }

        private void SetImageSaturation(Image image, float saturation)
        {
            if (image != null)
            {
                Color color = image.color;
                color.r = Mathf.Clamp01(saturation); // Adjust red channel
                image.color = color;
            }
        }
    }
}
