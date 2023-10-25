using System;
using System.Linq;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections.Generic;
using Realms;

namespace SyncOddly.Models;

public class RealmHelpers
{
    /**
     * Tries to return numWanted randomly selected out of the set given. Works best if much larger set.
     * @return empty or all set if less in set than wanted
     * @warning because this returns a list of Realm objects do NOT put it on a background thread as they don't trave across threads.
     */
    static public IEnumerable<RC> RandomSubset<RC>(int numWanted, Realm realm) where RC : RealmObject
    {
        Debug.Assert(numWanted > 0);
        var allRC = realm.All<RC>();
        var totalRC = allRC.Count();
        if (totalRC <= numWanted)
        {
            return allRC;
        }
        return RandomSubsetHelper(allRC, numWanted, totalRC, realm);
    }

    static public IEnumerable<RC> RandomSubset<RC>(IEnumerable<RC> from, int percentWanted, Realm realm) where RC : RealmObject
    {
        Debug.Assert(percentWanted > 0);
        var totalRC = from.Count();
        if (totalRC <= 1)
        {
            return from;
        }
        var numWanted = totalRC * percentWanted / 100;
        return RandomSubsetHelper(from, numWanted, totalRC, realm);
    }

    static private IEnumerable<RC> RandomSubsetHelper<RC>(IEnumerable<RC> from, int numWanted, int totalRC, Realm realm) where RC : RealmObject
    {
        Debug.Assert(numWanted > 0);
        var ret = new List<RC>();
        // kindof random by skipping roughly evenly through the data
        int maxRandomSkip = totalRC / numWanted + 1;  // really only works well if many more 
        if (maxRandomSkip < 3)
        {
            return from.Take(numWanted);
        }
        var numLeftToGet = numWanted;
        var rando = new Random();
        var getAt = rando.Next(maxRandomSkip) / 2;  // start by a half-skip
        while (numLeftToGet > 0)
        {
            RC nextRecord = from.ElementAt(getAt);
            ret.Add(nextRecord);
            numLeftToGet--;
            getAt = getAt + rando.Next(1, maxRandomSkip);  // MUST skip at least 1 forward otherwise return dups
        }
        return ret;
    }

    // unable to write a generic that is Either RealmObject OR EmbeddedObject so this copies RandomSubsetHelper
    static public IEnumerable<RC> RandomSubsetEmbedded<RC>(IEnumerable<RC> from, int percentWanted, Realm realm) where RC : EmbeddedObject
    {
        Debug.Assert(percentWanted > 0);
        var totalRC = from.Count();
        if (totalRC <= 1)
        {
            return from;
        }
        int numWanted = (int)Math.Round((totalRC * percentWanted + 0.9) / 100.0);
        var ret = new List<RC>();
        if (numWanted < 1)
        {
            return ret;
        }

        // kindof random by skipping roughly evenly through the data
        int maxRandomSkip = totalRC / numWanted + 1;  // really only works well if many more 
        if (maxRandomSkip < 2)
        {
            return from.Take(numWanted);
        }
        var numLeftToGet = numWanted;
        var rando = new Random();
        var getAt = rando.Next(maxRandomSkip) / 2;  // start by a half-skip
        while (numLeftToGet > 0)
        {
            RC nextRecord = from.ElementAt(getAt);
            ret.Add(nextRecord);
            numLeftToGet--;
            getAt = getAt + rando.Next(1, maxRandomSkip);  // MUST skip at least 1 forward otherwise return dups
        }
        return ret;
    }


}

