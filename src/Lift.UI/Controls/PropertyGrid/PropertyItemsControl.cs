﻿using System.Windows.Controls;

namespace Lift.UI.Controls;

public class PropertyItemsControl : ListBox
{
    protected override bool IsItemItsOwnContainerOverride(object item) => item is PropertyItem;

    public PropertyItemsControl()
    {
#if !NET40
        VirtualizingPanel.SetIsVirtualizingWhenGrouping(this, true);
        VirtualizingPanel.SetScrollUnit(this, ScrollUnit.Pixel);
#else
        System.Windows.Controls.ScrollViewer.SetCanContentScroll(this, false);
#endif
    }
}
