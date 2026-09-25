using System;
using UnityEngine;

namespace LudusSDK
{
    [Serializable]
    public sealed class LudusCapabilities
    {
        [Header("Interações automáticas")]

        [Tooltip("Registra cliques ou toques na tela.")]
        public bool clicks = true;

        [Tooltip("Registra o caminho do mouse ou toque.")]
        public bool mousePath = true;

        [Tooltip("Registra automaticamente início, percurso e fim de arrastes.")]
        public bool dragPath = true;

        [Tooltip("Campo reservado pelo contrato. Esta versão não coleta capturas de imagem automaticamente.")]
        public bool screenshots = false;

        [Tooltip("Campo reservado pelo contrato. Esta versão não registra períodos de inatividade automaticamente.")]
        public bool inactivity = false;

        [Tooltip("Campo reservado pelo contrato. Esta versão não registra mudanças de foco automaticamente.")]
        public bool focusEvents = false;

        [Header("Eventos informados pelo jogo")]

        [Tooltip("Campo reservado pelo contrato. Esta versão não oferece uma integração específica para fases.")]
        public bool phaseEvents = false;

        [Tooltip("Campo reservado pelo contrato. Esta versão não oferece uma integração específica para acertos e erros.")]
        public bool correctWrong = false;

        [Tooltip("Campo reservado pelo contrato. Esta versão não oferece uma integração específica para categorias.")]
        public bool categoryEvents = false;

        [Tooltip("O jogo pode registrar eventos semânticos próprios.")]
        public bool customEvents = true;

        public LudusCapabilities Clone()
        {
            return new LudusCapabilities
            {
                clicks = clicks,
                mousePath = mousePath,
                dragPath = dragPath,
                screenshots = screenshots,
                inactivity = inactivity,
                focusEvents = focusEvents,
                phaseEvents = phaseEvents,
                correctWrong = correctWrong,
                categoryEvents = categoryEvents,
                customEvents = customEvents,
            };
        }
    }
}
