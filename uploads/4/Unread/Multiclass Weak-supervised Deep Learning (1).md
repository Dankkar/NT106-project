# **Multiclass Weak-supervised Deep Learning**

---

## **Abstract**

The increasing sophistication of cyber threats in Internet of Things (IoT) networks necessitates the development of more effective Intrusion Detection Systems (IDS). However, Multiclass Novelty Detection (MND), which aims to classify both known and unknown attacks, remains an underexplored challenge in the IoT security domain. Existing methods either struggle to scale with large datasets or fail to effectively differentiate between known attack types. This project proposes a novel **Multiclass Weak-supervised Deep Learning model** that can simultaneously classify normal traffic, known attacks, and novel attacks. By leveraging weak supervision, the model aims to mitigate the limitations of conventional supervised learning approaches while improving generalization to unseen threats. The proposed approach is expected to enhance the adaptability and efficiency of IoT-IDS solutions, addressing real-world challenges such as concept drift and limited labeled data.

---

## **Introduction**

### **Concept Drift in IoT Intrusion Detection**

The rapid proliferation of IoT devices has introduced significant security vulnerabilities, as these devices often lack robust built-in protection mechanisms. Consequently, **Intrusion Detection Systems (IDSs)** have become crucial for safeguarding IoT networks. However, a critical limitation of current IDSs is their inability to effectively handle **Multiclass Novelty Detection (MND)**—the task of simultaneously classifying **known attack types** while identifying **novel (unknown) threats**. This challenge is raised by two primary factors: **limited labeled data** and **concept drift** which is challenges with supervised models. Due to the high cost and time-consuming nature of data annotation, most real-world IoT datasets contain **incomplete attack labels**. Additionally, attack patterns evolve over time, leading to concept drift, which further degrades the performance of traditional IDS models trained on static datasets.

Recent research has attempted to address the MND problem, but significant limitations remain. For instance, **nNFST**, a transformation-based approach, mitigates the loss of local structure in novelty detection but suffers from heavy computational costs and sensitivity to complex data distributions. Meanwhile, **weakly-supervised deep learning models** such as PreNET have demonstrated promise in detecting novel attacks without full supervision. However, these methods still lack the ability to **classify known attack types**, which is critical for practical deployment in IoT security. Thus, existing approaches either fail to scale efficiently or cannot differentiate between known and novel threats, limiting their effectiveness in real-world applications.

To overcome these challenges, this project aims to develop a **Multiclass Weak-supervised Deep Learning model** that enhances **MND** capabilities in IoT-IDS. By integrating weak supervision with deep learning, the proposed model seeks to **classify known attack types while simultaneously detecting novel threats**, addressing the limitations of previous methods. The model will be designed to adapt to concept drift and scale effectively with large, high-dimensional IoT datasets, making it a practical solution for real-world deployment.

### **Problem Statement**

Current IDS solutions lack an efficient approach to addressing **Multiclass Novelty Detection (MND)** within IoT networks. This research seeks to answer the question: **"How can we design a Multiclass Weak-supervised Deep Learning model to address the MND challenge in IoT networks?"**

### **Objectives and Contributions**

This project focuses on two key objectives:
- **Developing a Multiclass Weak-supervised Deep Learning model** that can effectively classify normal traffic, known attack types, and novel attacks.
- **Evaluating the model’s performance** across different IoT attack contexts, assessing its scalability, adaptability, and accuracy.

The key contributions include:
- **Enhanced classification capabilities:** Unlike previous approaches, the proposed model will be capable of classifying both known and novel attack types within an IoT-IDS framework.
- **Improved scalability and efficiency:** Leveraging weak supervision and deep learning techniques, the model will reduce reliance on fully labeled datasets while maintaining high accuracy.
- **Robust handling of concept drift:** The model will be designed to dynamically adapt to evolving attack patterns, making it suitable for real-world applications.

---

## **Background and Literature Review**

### **Current State of the Art**

Research [1] introduced **nNFST**, a transformation-based approach designed to enhance MND in IDS. While it preserves local structure and mitigates the impact of singular points, it remains **computationally expensive** and **sensitive to complex data distributions**, making it less suitable for large-scale IoT datasets.

Research [2] proposed **PreNET**, a weakly-supervised deep learning model that detects both known and unknown attacks. However, PreNET lacks the capability to **differentiate between known attack types**, limiting its practical application in real-world IDS scenarios.

### **Limitations of Existing Methods**
- **Scalability issues:** Some methods struggle with large-scale IoT datasets, requiring heavy computational resources.
- **Lack of labeled data:** Weakly-supervised models can detect unknown threats but often fail to classify known attack types.
- **Concept drift:** Existing IDS models lack adaptability, leading to reduced accuracy as new attack patterns emerge.

---

## **Methodology**

### **Model Design**

Our approach is inspired by **PreNET** [2], which learns **pairwise relational features** and **anomaly scores** by predicting the relationships between randomly sampled training instances (e.g., anomaly-anomaly, anomaly-unlabeled, or unlabeled-unlabeled). The key differences in our design include:
- **Customizing the loss function** to enable classification between known attack types, overcoming PreNET's primary limitation.
- **Enhancing scalability** to support large-scale IoT datasets while maintaining efficiency.

### **Evaluation Datasets**

The proposed model will be evaluated on **three IoT datasets** representing different attack contexts:
1. **BoT-IoT** – Large-scale dataset with diverse IoT attack types.
2. **CIC-IoT2023** – Real-world IoT dataset with multiple attack scenarios.
3. **N-BaIoT** – IoT-specific botnet attack dataset.

**Note:** The dataset could be changed later

---

## **Expected Evaluation Results**

Our experiments aim to answer the following key research questions:
1. **How does the proposed model perform when new IoT devices appear?**
2. **How does it handle the emergence of novel attack types?**
3. **How scalable is the model for high-dimensional data? (N=$10^6$, d=$10^4$)**

We expect that our model will:
- **Achieve high detection accuracy**, even as new devices and attack types emerge.
- **Outperform existing IDS solutions** in both adaptability and efficiency.
- **Demonstrate scalability**, maintaining high performance under large-scale network traffic.

---

## **Timeline**

| **Phase**                  | **Description**                                      | **Duration**  | **Start Date** | **End Date**   |
|----------------------------|------------------------------------------------------|--------------|--------------|--------------|
| Literature Review          | Survey existing research on IDS & concept drift.    | 1 month      | 01/2025      | 02/2025      |
| Benchmark Framework        | Build the benchmark setup for evaluation.           | 1 month      | 02/2025      | 03/2025      |
| Model Development          | Design & implement the weakly-supervised model.     | 3 months     | 03/2025      | 06/2025      |
| Experimental Evaluation    | Run tests on IoT datasets & analyze results.        | 2 months     | 06/2025      | 08/2025      |
| Paper Writing              | Prepare research paper & reports.                    | 2 months     | 08/2025      | 10/2025      |

---

## **References**

1. Nguyen, X. H., & Le, K. H. (2025). nNFST: A single-model approach for multiclass novelty detection in network intrusion detection systems. *Journal of Network and Computer Applications, 104128*.
2. Pang, G., Shen, C., Jin, H., & van den Hengel, A. (2023, August). Deep weakly-supervised anomaly detection. *Proceedings of the 29th ACM SIGKDD Conference on Knowledge Discovery and Data Mining*, 1795-1807.

---

