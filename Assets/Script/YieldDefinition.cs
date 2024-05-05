using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    internal static class YieldDefinition
    {
        class FloatComparer : IEqualityComparer<float>
        {
            bool IEqualityComparer<float>.Equals(float x, float y)
            {
                return x == y;
            }
            int IEqualityComparer<float>.GetHashCode(float obj)
            {
                return obj.GetHashCode();
            }
        }

        private static readonly Dictionary<float, WaitForSeconds> _timeInterval = 
            new Dictionary<float, WaitForSeconds>(new FloatComparer());

        private static readonly Dictionary<float, WaitForSecondsRealtime> _timeIntervalRealTime =
            new Dictionary<float, WaitForSecondsRealtime>(new FloatComparer());

        /// <summary>
        /// 기존 WaitForSeconds 를 캐싱하여 가비지를 매번 생성하지않음
        /// <para>WaitForSeconds : 사용 시 seconds 만큼 기다림</para>
        /// </summary>
        /// <param name="seconds">기다리는 시간</param>
        /// <returns></returns>
        public static WaitForSeconds WaitForSeconds(float seconds)
        {
            WaitForSeconds wfs;
            if(!_timeInterval.TryGetValue(seconds, out wfs))
                _timeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
            return wfs;
        }

        /// <summary>
        /// 기존 WaitForSecondsRealtime 을 캐싱하여 가비지를 매번 생성하지않음
        /// <para>WaitForSecondsRealtime : 사용 시 실제시간으로 seconds 만큼 기다림</para>
        /// </summary>
        /// <param name="seconds">기다리는 시간</param>
        /// <returns></returns>
        public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
        {
            WaitForSecondsRealtime wfsRealtime;
            if(!_timeIntervalRealTime.TryGetValue(seconds, out wfsRealtime))
                _timeIntervalRealTime.Add(seconds, wfsRealtime = new WaitForSecondsRealtime(seconds));
            return wfsRealtime;
        }
    }
}