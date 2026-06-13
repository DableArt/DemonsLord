using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Набор береговых тайлов с подбором по направлению воды.
/// Отображается в инспекторе как именованные поля внутри <see cref="WorldSettings"/>.
/// </summary>
[System.Serializable]
public class ShoreConfig
{
    // -----------------------------------------------------------------------
    // Кардинальные стороны
    // -----------------------------------------------------------------------
    [Header("Кардинальные стороны")]
    [Tooltip("Вода сверху — береговой переход с верхним краем к воде")]
    public TileBase top;

    [Tooltip("Вода снизу — береговой переход с нижним краем к воде")]
    public TileBase bottom;

    [Tooltip("Вода слева — береговой переход с левым краем к воде")]
    public TileBase left;

    [Tooltip("Вода справа — береговой переход с правым краем к воде")]
    public TileBase right;

    // -----------------------------------------------------------------------
    // Диагональные углы (вода только по диагонали, кардинальных соседей нет)
    // -----------------------------------------------------------------------
    [Header("Диагональные углы")]
    [Tooltip("Вода только по диагонали сверху-слева")]
    public TileBase topLeft;

    [Tooltip("Вода только по диагонали сверху-справа")]
    public TileBase topRight;

    [Tooltip("Вода только по диагонали снизу-слева")]
    public TileBase bottomLeft;

    [Tooltip("Вода только по диагонали снизу-справа")]
    public TileBase bottomRight;

    // -----------------------------------------------------------------------
    // Резервный тайл
    // -----------------------------------------------------------------------
    [Header("Резервный тайл")]
    [Tooltip("Используется при нескольких сторонах воды или если конкретный "
           + "направленный тайл не назначен. Также играет роль единственного "
           + "берегового тайла, если директивные тайлы не заполнены.")]
    public TileBase fallback;

    // -----------------------------------------------------------------------
    // Метод выбора тайла
    // -----------------------------------------------------------------------

    /// <summary>
    /// Возвращает береговой тайл, подобранный по маске водных соседей.
    /// <para>Приоритет: одна кардинальная сторона → одна диагональ → <see cref="fallback"/>.</para>
    /// Если итоговый тайл <c>null</c>, вызывающий код должен использовать fallback земли.
    /// </summary>
    /// <param name="waterTop">Вода сверху (y+1)</param>
    /// <param name="waterBottom">Вода снизу (y-1)</param>
    /// <param name="waterLeft">Вода слева (x-1)</param>
    /// <param name="waterRight">Вода справа (x+1)</param>
    /// <param name="waterTopLeft">Вода по диагонали сверху-слева</param>
    /// <param name="waterTopRight">Вода по диагонали сверху-справа</param>
    /// <param name="waterBottomLeft">Вода по диагонали снизу-слева</param>
    /// <param name="waterBottomRight">Вода по диагонали снизу-справа</param>
    public TileBase Resolve(
        bool waterTop,    bool waterBottom,
        bool waterLeft,   bool waterRight,
        bool waterTopLeft,    bool waterTopRight,
        bool waterBottomLeft, bool waterBottomRight)
    {
        int cardinals = (waterTop    ? 1 : 0) + (waterBottom ? 1 : 0)
                      + (waterLeft   ? 1 : 0) + (waterRight  ? 1 : 0);

        // Ровно одна кардинальная сторона — используем направленный тайл
        if (cardinals == 1)
        {
            TileBase cardinal = waterTop    ? top    :
                                waterBottom ? bottom :
                                waterLeft   ? left   : right;
            if (cardinal != null) return cardinal;
        }

        // Нет кардинальных соседей — проверяем ровно одну диагональ
        if (cardinals == 0)
        {
            int diags = (waterTopLeft    ? 1 : 0) + (waterTopRight   ? 1 : 0)
                      + (waterBottomLeft ? 1 : 0) + (waterBottomRight ? 1 : 0);
            if (diags == 1)
            {
                TileBase diag = waterTopLeft    ? topLeft    :
                                waterTopRight   ? topRight   :
                                waterBottomLeft ? bottomLeft : bottomRight;
                if (diag != null) return diag;
            }
        }

        // Несколько направлений или конкретный тайл не назначен — резерв
        return fallback;
    }
}
