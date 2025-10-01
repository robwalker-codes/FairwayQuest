using System.Collections.Generic;
using FairwayQuest.Core.Models;

namespace FairwayQuest.Core.Courses;

public static class DefaultCourseProvider
{
    public static IReadOnlyList<Hole> CreateCourse(int holeCount)
    {
        return holeCount switch
        {
            9 => NineHoleCourse,
            18 => EighteenHoleCourse,
            _ => throw new ArgumentOutOfRangeException(nameof(holeCount), "Only 9 or 18 hole courses are supported.")
        };
    }

    private static readonly List<Hole> NineHoleCourse = new()
    {
        new Hole { Number = 1, Par = 4, Yardage = 360, StrokeIndex = 4 },
        new Hole { Number = 2, Par = 3, Yardage = 145, StrokeIndex = 8 },
        new Hole { Number = 3, Par = 5, Yardage = 520, StrokeIndex = 2 },
        new Hole { Number = 4, Par = 4, Yardage = 410, StrokeIndex = 6 },
        new Hole { Number = 5, Par = 3, Yardage = 178, StrokeIndex = 7 },
        new Hole { Number = 6, Par = 4, Yardage = 390, StrokeIndex = 3 },
        new Hole { Number = 7, Par = 5, Yardage = 505, StrokeIndex = 1 },
        new Hole { Number = 8, Par = 3, Yardage = 150, StrokeIndex = 9 },
        new Hole { Number = 9, Par = 4, Yardage = 430, StrokeIndex = 5 }
    };

    private static readonly List<Hole> EighteenHoleCourse = new()
    {
        new Hole { Number = 1, Par = 4, Yardage = 405, StrokeIndex = 10 },
        new Hole { Number = 2, Par = 3, Yardage = 168, StrokeIndex = 16 },
        new Hole { Number = 3, Par = 5, Yardage = 520, StrokeIndex = 4 },
        new Hole { Number = 4, Par = 4, Yardage = 440, StrokeIndex = 2 },
        new Hole { Number = 5, Par = 4, Yardage = 360, StrokeIndex = 12 },
        new Hole { Number = 6, Par = 3, Yardage = 198, StrokeIndex = 14 },
        new Hole { Number = 7, Par = 5, Yardage = 555, StrokeIndex = 6 },
        new Hole { Number = 8, Par = 4, Yardage = 415, StrokeIndex = 8 },
        new Hole { Number = 9, Par = 3, Yardage = 150, StrokeIndex = 18 },
        new Hole { Number = 10, Par = 4, Yardage = 430, StrokeIndex = 9 },
        new Hole { Number = 11, Par = 3, Yardage = 190, StrokeIndex = 15 },
        new Hole { Number = 12, Par = 5, Yardage = 575, StrokeIndex = 3 },
        new Hole { Number = 13, Par = 4, Yardage = 380, StrokeIndex = 11 },
        new Hole { Number = 14, Par = 4, Yardage = 425, StrokeIndex = 7 },
        new Hole { Number = 15, Par = 3, Yardage = 210, StrokeIndex = 17 },
        new Hole { Number = 16, Par = 5, Yardage = 505, StrokeIndex = 5 },
        new Hole { Number = 17, Par = 4, Yardage = 445, StrokeIndex = 1 },
        new Hole { Number = 18, Par = 4, Yardage = 395, StrokeIndex = 13 }
    };
}
