using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CS_Birth : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown monthDropdown;
    [SerializeField] private TMP_Dropdown dayDropdown;

    private static readonly int[] DaysInMonth = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    public int SelectedMonth => monthDropdown.value + 1;
    public int SelectedDay   => dayDropdown.value + 1;

    private void Awake()
    {
        PopulateMonths();
        monthDropdown.onValueChanged.AddListener(_ => RefreshDays());
        RefreshDays();
    }

    private void PopulateMonths()
    {
        monthDropdown.ClearOptions();
        var options = new List<string>();
        for (int m = 1; m <= 12; m++)
            options.Add($"{m}");
        monthDropdown.AddOptions(options);
    }

    private void RefreshDays()
    {
        int prevDay  = dayDropdown.value;
        int maxDays  = DaysInMonth[monthDropdown.value];

        dayDropdown.ClearOptions();
        var options = new List<string>();
        for (int d = 1; d <= maxDays; d++)
            options.Add($"{d}");
        dayDropdown.AddOptions(options);

        dayDropdown.value = Mathf.Clamp(prevDay, 0, maxDays - 1);
        dayDropdown.RefreshShownValue();
    }
}
