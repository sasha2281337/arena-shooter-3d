using UnityEngine;

namespace ArenaShooter.Audio
{
    public static class AudioFeedbackUtility
    {
        public static AudioClip PickRandomClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }

            int validCount = 0;

            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                return null;
            }

            int pick = Random.Range(0, validCount);

            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null)
                {
                    continue;
                }

                if (pick == 0)
                {
                    return clips[i];
                }

                pick--;
            }

            return null;
        }

        public static void PlayDetached(AudioClip clip, Vector3 position, float volume, bool playAs2D)
        {
            if (clip == null)
            {
                return;
            }

            GameObject audioObject = new GameObject($"OneShotAudio_{clip.name}");
            audioObject.transform.position = position;

            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = playAs2D ? 0f : 1f;
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();

            Object.Destroy(audioObject, clip.length + 0.1f);
        }
    }
}
