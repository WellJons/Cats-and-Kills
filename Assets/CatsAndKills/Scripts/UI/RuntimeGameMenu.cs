using CatsAndKills.Core;
using CatsAndKills.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CatsAndKills.UI
{
    public sealed class RuntimeGameMenu : MonoBehaviour
    {
        private enum Page
        {
            Briefing,
            Playing,
            Pause,
            Settings,
            Complete
        }

        private Page _page = Page.Briefing;
        private Page _settingsReturnPage = Page.Pause;

        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _button;
        private GUIStyle _center;
        private MissionDirector _mission;

        public bool GameplayBlocked => _page != Page.Playing;

        private void Start()
        {
            GamePreferences.Apply();

            if (CheckpointSystem.HasCheckpoint)
            {
                _page = Page.Playing;
                Time.timeScale = 1f;
            }
            else
            {
                Time.timeScale = 0f;
            }
        }

        private void Update()
        {
            if (_mission == null)
                _mission = FindFirstObjectByType<MissionDirector>();

            if (_mission != null && _mission.MissionComplete && _page != Page.Complete)
            {
                _page = Page.Complete;
                Time.timeScale = 0f;
            }

            bool pause =
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

            if (!pause) return;

            if (_page == Page.Playing)
            {
                _page = Page.Pause;
                Time.timeScale = 0f;
            }
            else if (_page == Page.Pause)
            {
                Resume();
            }
            else if (_page == Page.Settings)
            {
                _page = _settingsReturnPage;
            }
        }

        private void EnsureStyles()
        {
            if (_title != null) return;

            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 40,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _title.normal.textColor = Color.white;

            _body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                wordWrap = true,
                alignment = TextAnchor.UpperCenter
            };
            _body.normal.textColor = new Color(0.82f, 0.86f, 0.94f);

            _button = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fixedHeight = 42f
            };

            _center = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            _center.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            if (_page == Page.Playing) return;
            EnsureStyles();

            Color old = GUI.color;
            GUI.color = new Color(0.015f, 0.018f, 0.028f, 0.93f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = old;

            float width = Mathf.Min(680f, Screen.width - 60f);
            float x = (Screen.width - width) * 0.5f;

            switch (_page)
            {
                case Page.Briefing:
                    DrawBriefing(x, width);
                    break;
                case Page.Pause:
                    DrawPause(x, width);
                    break;
                case Page.Settings:
                    DrawSettings(x, width);
                    break;
                case Page.Complete:
                    DrawComplete(x, width);
                    break;
            }
        }

        private void DrawBriefing(float x, float width)
        {
            GUI.Label(new Rect(x, 90, width, 60), "CATS AND KILLS", _title);
            GUI.Label(
                new Rect(x + 40, 170, width - 80, 130),
                "ОПЕРАЦИЯ 01 // НОЧНОЙ ОБЪЕКТ\n\nПроникнуть на территорию, добраться до архивного терминала и уйти через южный сектор. Противник организован в группы и передаёт контакт между бойцами.",
                _body);

            if (GUI.Button(new Rect(x + 180, 330, width - 360, 44), "НАЧАТЬ ОПЕРАЦИЮ", _button))
                Begin();

            if (GUI.Button(new Rect(x + 180, 386, width - 360, 44), "НАСТРОЙКИ", _button))
            {
                _settingsReturnPage = Page.Briefing;
                _page = Page.Settings;
            }
        }

        private void DrawPause(float x, float width)
        {
            GUI.Label(new Rect(x, 110, width, 60), "ПАУЗА", _title);

            if (GUI.Button(new Rect(x + 180, 220, width - 360, 44), "ПРОДОЛЖИТЬ", _button))
                Resume();

            if (GUI.Button(new Rect(x + 180, 276, width - 360, 44), "НАСТРОЙКИ", _button))
            {
                _settingsReturnPage = Page.Pause;
                _page = Page.Settings;
            }

            if (GUI.Button(new Rect(x + 180, 332, width - 360, 44), "ПЕРЕЗАПУСТИТЬ УРОВЕНЬ", _button))
            {
                CheckpointSystem.Clear();
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void DrawSettings(float x, float width)
        {
            GUI.Label(new Rect(x, 90, width, 60), "НАСТРОЙКИ", _title);

            GUI.Label(new Rect(x + 100, 190, width - 200, 30), $"Громкость: {GamePreferences.MasterVolume * 100f:0}%", _center);
            float volume = GUI.HorizontalSlider(
                new Rect(x + 150, 230, width - 300, 24),
                GamePreferences.MasterVolume,
                0f,
                1f);

            if (!Mathf.Approximately(volume, GamePreferences.MasterVolume))
                GamePreferences.MasterVolume = volume;

            GUI.Label(new Rect(x + 100, 270, width - 200, 30), $"Тряска камеры: {GamePreferences.ScreenShake * 100f:0}%", _center);
            float shake = GUI.HorizontalSlider(
                new Rect(x + 150, 310, width - 300, 24),
                GamePreferences.ScreenShake,
                0f,
                1f);

            if (!Mathf.Approximately(shake, GamePreferences.ScreenShake))
                GamePreferences.ScreenShake = shake;

            bool haptics = GUI.Toggle(
                new Rect(x + 210, 355, width - 420, 30),
                GamePreferences.Haptics,
                " Вибрация геймпада");

            if (haptics != GamePreferences.Haptics)
                GamePreferences.Haptics = haptics;

            if (GUI.Button(new Rect(x + 180, 420, width - 360, 44), "НАЗАД", _button))
                _page = _settingsReturnPage;
        }

        private void DrawComplete(float x, float width)
        {
            GUI.Label(new Rect(x, 105, width, 60), "ОПЕРАЦИЯ ЗАВЕРШЕНА", _title);
            GUI.Label(
                new Rect(x + 50, 190, width - 100, 100),
                "Архив получен. Объект покинут. Это конец первого vertical slice — не конец истории.",
                _body);

            if (GUI.Button(new Rect(x + 180, 320, width - 360, 44), "СЫГРАТЬ ЕЩЁ РАЗ", _button))
            {
                CheckpointSystem.Clear();
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void Begin()
        {
            _page = Page.Playing;
            Time.timeScale = 1f;
        }

        private void Resume()
        {
            _page = Page.Playing;
            Time.timeScale = 1f;
        }

        private void OnDisable()
        {
            if (Time.timeScale == 0f)
                Time.timeScale = 1f;
        }
    }
}
