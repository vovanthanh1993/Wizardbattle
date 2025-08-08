using System;
using System.Collections.Generic;
using FancyScrollView;
using UnityEngine;

public class RoomScrollView : FancyScrollRect<RoomData, RoomContext>
{
    [SerializeField]
    private RectTransform _contentPanel = default;

    [SerializeField]
    private float _cellSize = 100f;

    [SerializeField]
    private GameObject _cellPrefab = default;

    protected override float CellSize => this._cellSize;

    protected override GameObject CellPrefab => this._cellPrefab;

    public void UpdateData(IList<RoomData> items)
    {
        this.UpdateContents(items);

        var size = this._contentPanel.sizeDelta;
        float contentHeight = (this._cellSize + this.spacing) * items.Count;
        size.y = contentHeight < 0 ? 0 : contentHeight;
        this._contentPanel.sizeDelta = size;

        this.Relayout();
    }

    public void OnCellClicked(Action<int> callback)
    {
        this.Context.OnCellClicked = callback;
    }
}

