using System;

namespace MurderMystery
{

    public interface IUseCard
    {
        void UseCard(ulong playerId, CardObject data, Action<CardIntent> action = null);
        CardIntent OnCardSelected();
    }
}