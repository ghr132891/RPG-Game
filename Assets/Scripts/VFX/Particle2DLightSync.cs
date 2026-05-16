using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class Particle2DLightSync : MonoBehaviour
{
    [Header("把做好的 Firefly_LightSource 预制体拖到这里")]
    public Light2D lightPrefab;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private List<Light2D> activeLights = new List<Light2D>();

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    void LateUpdate()
    {
        if (lightPrefab == null) return;

        int aliveCount = ps.GetParticles(particles);

        // 1. 如果光不够，动态克隆预制体
        while (activeLights.Count < aliveCount)
        {
            // 【核心修复】：去掉 transform 参数！不作为子物体生成，彻底杜绝继承任何错误的旋转和缩放！
            Light2D newLight = Instantiate(lightPrefab);
            // 强行把光摆正，永远正对 2D 摄像机
            newLight.transform.rotation = Quaternion.identity;
            newLight.transform.localScale = Vector3.one;
            activeLights.Add(newLight);
        }

        // 2. 如果光多了，隐藏多余的光
        for (int i = aliveCount; i < activeLights.Count; i++)
        {
            activeLights[i].gameObject.SetActive(false);
        }

        // 3. 将光的位置死死绑定在存活的粒子上
        for (int i = 0; i < aliveCount; i++)
        {
            activeLights[i].gameObject.SetActive(true);

            if (ps.main.simulationSpace == ParticleSystemSimulationSpace.World)
                activeLights[i].transform.position = particles[i].position;
            else
                activeLights[i].transform.position = transform.TransformPoint(particles[i].position);
        }
    }

    // 【新增】：当粒子系统被销毁时，清理掉所有独立生成的光，防止内存泄漏
    private void OnDestroy()
    {
        foreach (var light in activeLights)
        {
            if (light != null) Destroy(light.gameObject);
        }
    }
}
