using FancyScrollView;
using System;

public class RoomContext : FancyScrollRectContext
{
    public int SelectedIndex { get; set; } = -1;
    public Action<int> OnCellClicked { get; set; }
}