using Frontline.Data.Entities;
using Frontline.Game.Card;

namespace Frontline.Extensions;

public static class EntityExtensions
{
    extension(ItemEntity)
    {
        public static ItemEntity FromTemplate(CardTemplate template)
        {
            return new ItemEntity
            {
                TemplateId = template.CardId,
                Rank = (sbyte) template.MinimumRank
            };
        }
    }
}