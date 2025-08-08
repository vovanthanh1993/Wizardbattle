using FancyScrollView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomCell : FancyScrollRectCell<RoomData, RoomContext>
{
    [SerializeField]
    private TextMeshProUGUI _roomNameText = default;

    [SerializeField]
    private TextMeshProUGUI _playerCountText = default;

    [SerializeField]
    private Button _button = default;

    public override void Initialize()
    {
        _button.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));
    }

    public override void UpdateContent(RoomData itemData)
    {
        this._roomNameText.text = itemData.RoomName;
        this._playerCountText.text = $"{itemData.PlayerCount} / {itemData.MaxPlayers}";
    }

    protected override void UpdatePosition(float normalizedPosition, float localPosition)
    {
        base.UpdatePosition(normalizedPosition, localPosition);
    }
}
