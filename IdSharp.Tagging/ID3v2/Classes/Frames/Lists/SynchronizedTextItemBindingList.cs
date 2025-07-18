using System.Collections.Generic;
using System.ComponentModel;
using IdSharp.Tagging.ID3v2.Frames.Items;

namespace IdSharp.Tagging.ID3v2.Frames.Lists;

internal sealed class SynchronizedTextItemBindingList : BindingList<ISynchronizedTextItem>
{
    public SynchronizedTextItemBindingList()
    {
        AllowNew = true;
    }

    public SynchronizedTextItemBindingList(IList<ISynchronizedTextItem> synchronizedTextItemList)
        : base(synchronizedTextItemList)
    {
        AllowNew = true;
    }

    protected override object AddNewCore()
    {
        ISynchronizedTextItem synchronizedTextItem = new SynchronizedTextItem();
        Add(synchronizedTextItem);

        // Not necessary to hook up event handlers, base class calls InsertItem

        return synchronizedTextItem;
    }

    protected override void InsertItem(int index, ISynchronizedTextItem item)
    {
        //item.DescriptionChanging += new EventHandler<CancelDataEventArgs<String>>(AttachedPicture_DescriptionChanging);
        //item.PictureTypeChanging += new EventHandler<CancelDataEventArgs<PictureType>>(AttachedPicture_PictureTypeChanging);

        base.InsertItem(index, item);
    }
}
