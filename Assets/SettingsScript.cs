using UnityEngine;

public class SettingsScript : MonoBehaviour
{
    void Awake()
    {
        //ограничение фпс, чтобы видюха не шумела
        QualitySettings.vSyncCount = 1; //автоматическая адаптация под герцовку
       
        DontDestroyOnLoad(gameObject); //сохранение при переключении сцен
    }
}
