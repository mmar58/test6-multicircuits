import html2canvas from "html2canvas";
import { jsPDF } from "jspdf";

export async function exportToPng(svgElement: SVGSVGElement, circuitName: string) {
    try {
        // Create a wrapper div to render the SVG properly via html2canvas
        const wrapper = document.createElement('div');
        wrapper.style.position = 'absolute';
        wrapper.style.top = '-9999px';
        wrapper.style.width = svgElement.clientWidth + 'px';
        wrapper.style.height = svgElement.clientHeight + 'px';
        
        // Clone SVG and compute bounds to frame it
        const svgClone = svgElement.cloneNode(true) as SVGSVGElement;
        
        // We'll just render the current view of the canvas for simplicity
        wrapper.appendChild(svgClone);
        document.body.appendChild(wrapper);
        
        const canvas = await html2canvas(wrapper, {
            backgroundColor: "#0a0a0f", // Match theme
            scale: 2 // High res
        });
        
        document.body.removeChild(wrapper);
        
        const dataUrl = canvas.toDataURL("image/png");
        const link = document.createElement("a");
        link.download = `${circuitName.replace(/\s+/g, '_')}_Circuit.png`;
        link.href = dataUrl;
        link.click();
    } catch (e) {
        console.error("Export to PNG failed", e);
    }
}

export async function exportToPdf(svgElement: SVGSVGElement, circuitName: string) {
    try {
        const wrapper = document.createElement('div');
        wrapper.style.position = 'absolute';
        wrapper.style.top = '-9999px';
        wrapper.style.width = svgElement.clientWidth + 'px';
        wrapper.style.height = svgElement.clientHeight + 'px';
        
        const svgClone = svgElement.cloneNode(true) as SVGSVGElement;
        wrapper.appendChild(svgClone);
        document.body.appendChild(wrapper);
        
        const canvas = await html2canvas(wrapper, {
            backgroundColor: "#0a0a0f",
            scale: 2
        });
        
        document.body.removeChild(wrapper);
        
        const imgData = canvas.toDataURL("image/png");
        
        // A4 landscape: 297 x 210 mm
        const pdf = new jsPDF({
            orientation: "landscape",
            unit: "mm",
            format: "a4"
        });
        
        // Add branding
        pdf.setFillColor(10, 10, 15);
        pdf.rect(0, 0, 297, 210, 'F');
        
        pdf.setTextColor(255, 255, 255);
        pdf.setFontSize(24);
        pdf.text(circuitName, 20, 20);
        
        pdf.setFontSize(12);
        pdf.setTextColor(150, 150, 150);
        pdf.text(`LogicFlow Export - ${new Date().toLocaleDateString()}`, 20, 30);
        
        // Calculate aspect ratio fit
        const imgProps = pdf.getImageProperties(imgData);
        const pdfWidth = 257; // 297 - 40 margin
        const pdfHeight = (imgProps.height * pdfWidth) / imgProps.width;
        
        pdf.addImage(imgData, 'PNG', 20, 40, pdfWidth, pdfHeight);
        
        pdf.save(`${circuitName.replace(/\s+/g, '_')}_Circuit.pdf`);
    } catch (e) {
        console.error("Export to PDF failed", e);
    }
}
