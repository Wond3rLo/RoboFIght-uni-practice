using UnityEngine;



public class PanelsController : MonoBehaviour
{
    
    [Header("Screen References")]
    public GameObject Hangar_Panel; //экран кастомизации стратегии автобоя (мб ещё позже и выбор робота)
    public GameObject Battle_Panel; //экран собственно боя

    public void StartBattle() //метод для боя
    {
        Hangar_Panel.SetActive(false);
        Battle_Panel.SetActive(true);
    }
    public void OpenHangar() //метод для ангара
    {
        Hangar_Panel.SetActive(true);
        Battle_Panel.SetActive(false);
    }

    void Start()
    {
        OpenHangar();
    }
}
